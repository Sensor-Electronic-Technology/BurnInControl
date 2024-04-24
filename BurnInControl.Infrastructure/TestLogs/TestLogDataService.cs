using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.ComDefinitions;
using ErrorOr;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
namespace BurnInControl.Infrastructure.TestLogs;

public class TestLogDataService {
    private readonly IMongoCollection<BurnInTestLog> _testLogCollection;
    private readonly StationDataService _stationDataService;
    
    public TestLogDataService(IMongoClient client,StationDataService stationDataService,IOptions<DatabaseSettings> settings){
        var database = client.GetDatabase(settings.Value.DatabaseName?? "burn_in_db");
        this._testLogCollection=database.GetCollection<BurnInTestLog>(settings.Value.TestLogCollectionName ?? "test_logs");
        this._stationDataService = stationDataService;
    }

    /*public async Task<ErrorOr<StationSavedState>> LoadFromSavedState(string stationId) {
        //this._savedStatesCollection.Find(e=>e.)
    }*/
    
    public TestLogDataService(IMongoClient client,StationDataService stationDataService){
        var database = client.GetDatabase("burn_in_db");
        this._testLogCollection=database.GetCollection<BurnInTestLog>("test_logs");
        this._stationDataService = stationDataService;
    }

    public async Task<ErrorOr<BurnInTestLog>> LoadSavedLog(ObjectId testId) {
        var log = await this._testLogCollection.Find(e => e._id == testId).Project(e=>new BurnInTestLog() {
                _id=e._id,
                StationId=e.StationId,
                SetCurrent=e.SetCurrent,
                RunTime=e.RunTime,
                StartTime=e.StartTime,
                StopTime=e.StopTime,
                Completed=e.Completed,
                TestSetup = e.TestSetup
            }).FirstOrDefaultAsync();
        return log != null ? log : Error.NotFound(description: "Log not found");
    }
    
    public async Task<List<string>> GetNotCompleted(string stationId) {
        return await this._testLogCollection.Find(e=>e.StationId==stationId && !e.Completed)
            .Project(e=>e._id.ToString())
            .ToListAsync();
    }
    
    public async Task<ErrorOr<BurnInTestLog>> GetTestLogNoReadings(ObjectId id) {
        var log=await this._testLogCollection.Find(e => e._id == id)
            .Project(e=>new BurnInTestLog() {
            _id=e._id,
            StationId=e.StationId,
            SetCurrent=e.SetCurrent,
            RunTime=e.RunTime,
            StartTime=e.StartTime,
            StopTime=e.StopTime,
            Completed=e.Completed,
            TestSetup = e.TestSetup
        }).FirstOrDefaultAsync();
        if (log == null) {
            return Error.NotFound(description: "BurnInTestLog Not Found");
        } else {
            return log;
        }
    }

    
    public async Task<ErrorOr<BurnInTestLog>> GetTestLogWithReadings(ObjectId id) {
        var log=await this._testLogCollection.Find(e => e._id == id).FirstOrDefaultAsync();
        if (log == null) {
            return Error.NotFound(description: "BurnInTestLog Not Found");
        } else {
            return log;
        }
    }

    
    public Task<bool> LogExists(ObjectId id) {
        return this._testLogCollection.Find(e => e._id == id)
            .AnyAsync();
    }
    public async Task<ErrorOr<BurnInTestLog>> StartNew(BurnInTestLog log) {
        log._id = ObjectId.GenerateNewId();
        var result = await this._stationDataService.GetTestConfiguration(log.SetCurrent);
        if (result.IsError) {
            return Error.NotFound(description:$"Test configuration for {log.SetCurrent.Name} not found");
        }
        log.RunTime=result.Value.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        return log;
    }
    public async Task<ErrorOr<BurnInTestLog>> StartNewUnknown(BurnInTestLog log,StationCurrent current) {
        var result = await this._stationDataService.GetTestConfiguration(current);
        if (result.IsError) {
            return Error.NotFound(description:$"Test configuration for {current.Value} not found");
        }
        log.SetCurrent=result.Value.SetCurrent;
        log.RunTime=result.Value.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        return log;
    }
    public async Task<ErrorOr<Deleted>> DeleteTestLog(ObjectId id) {
        var deleteResult = await this._testLogCollection.DeleteOneAsync(e => e._id == id);
        if (deleteResult.IsAcknowledged) {
            if(deleteResult.DeletedCount>0){
                return Result.Deleted;
            }
            return Error.Unexpected(description: "Log may not have been deleted");
        }
        return Error.Failure(description: "Failed to delete log");
    }
    
    public async Task<ErrorOr<Success>> SetStart(ObjectId id,DateTime start,StationSerialData data) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var updateBuilder = Builders<BurnInTestLog>.Update;
        var update=updateBuilder.Set(e => e.StartTime,start)
            .Push(e => e.Readings,new StationReading() {
                TimeStamp = start,
                Data=data
            });
        var updateResult=await this._testLogCollection.UpdateOneAsync(filter, update);
        if (updateResult.IsAcknowledged) {
            return Result.Success;
        }
        return Error.Failure(description: "Failed Mart Test as Running");
    }
    
    public async Task<ErrorOr<Created>> InsertReading(ObjectId logId,StationSerialData data) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,logId);
        var updateBuilder = Builders<BurnInTestLog>.Update;
        var update=updateBuilder.Push(e => e.Readings,new StationReading() {
            TimeStamp = DateTime.Now,
            Data=data
        });
        var readingResult = await this._testLogCollection.UpdateOneAsync(filter, update);
        if(readingResult.IsAcknowledged){
            if(readingResult.ModifiedCount>0){
                return Result.Created;
            } else {
                return Error.Unexpected(description: "Unknown State.  Reading may not have been inserted");
            }
        } else {
            return Error.Failure(description: "Failed to insert reading");
        }

    }
    
    public async Task<ErrorOr<Success>> SetCompleted(ObjectId id,string stationId,DateTime stop) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var update = Builders<BurnInTestLog>.Update
            .Set(e => e.StopTime,stop)
            .Set(e=>e.Completed,true);
        var clearResult =await this._stationDataService.ClearRunningTest(stationId);
        var logResult = await this._testLogCollection.UpdateOneAsync(filter, update);

        if (!clearResult.IsError && logResult.IsAcknowledged) {
            return Result.Success;
        }
        if (!clearResult.IsError && !logResult.IsAcknowledged) {
            return Error.Unexpected(description: "Unknown State. Log may not be finalized with Completed flag and StopTime");
        }
        if (clearResult.IsError && logResult.IsAcknowledged) {
            return Error.Unexpected(description: "Unknown State. Log was finalize but station Running flag was not cleared");
        }
        return Error.Failure(description: "Station Running flag not cleared and Log was not finalized");
    }
}
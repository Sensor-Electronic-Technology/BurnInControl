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
    
    public TestLogDataService(IMongoClient client,StationDataService stationDataService){
        var database = client.GetDatabase("burn_in_db");
        this._testLogCollection=database.GetCollection<BurnInTestLog>("test_logs");
        this._stationDataService = stationDataService;
    }
    
    public async Task<List<string>> GetNotCompleted(string stationId) {
        return await this._testLogCollection.Find(e=>e.StationId==stationId && !e.Completed)
            .Project(e=>e._id.ToString())
            .ToListAsync();
    }
    
    public async Task<ErrorOr<BurnInTestLog>> GetTestLog(ObjectId id) {
        var log=await this._testLogCollection.Find(e => e._id == id)
            .FirstOrDefaultAsync();
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

    public Task<bool> LogExistsAndNotCompleted(string testId) {
        return this._testLogCollection.Find(e => e._id == ObjectId.Parse(testId) && !e.Completed)
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
        var logExists = await this.LogExists(log._id);
        if(logExists){
            var stationTaskResult=await this._stationDataService.SetRunningTest(log.StationId, log._id);
            if(stationTaskResult.IsError){
                var deleteResult=await this.DeleteTestLog(log._id);
                if(deleteResult.IsError){
                    return Error.Failure(description: "Failed to mark test as running.  Log was inserted, you must delete before trying again");
                } else {
                    return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
                }
            }
            return log;
        } else {
            return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
        }
    }

    public async Task<ErrorOr<BurnInTestLog>> TryContinueFrom(string stationId) {
        var result=await this._stationDataService.CheckForRunningTest(stationId);
        if (!result.IsError) {
            var log = await this._testLogCollection.Find(e => e.StationId == stationId && e._id == result.Value)
                .FirstOrDefaultAsync();
            return log == null ? Error.NotFound() : log;
        } else {
            return Error.Failure();
        }
    }
    
    public async Task<ErrorOr<BurnInTestLog>> TryContinueFrom(string stationId,ObjectId testId) {
        var result=await this._stationDataService.CheckForRunningTest(stationId);
        if (!result.IsError) {
                if(result.Value!=testId) {
                    //TestId!=Station TestId
                    return Error.Conflict();
                }
                var log = await this._testLogCollection.Find(e => e.StationId == stationId && e._id ==testId)
                    .FirstOrDefaultAsync();
                return log == null ? Error.NotFound() : log;
        } else {
            //No Running Test
            return Error.Failure();
        }
    }
    
    public async Task<ErrorOr<BurnInTestLog>> LoadStationLog(string stationId) {
        var result=await this._stationDataService.CheckForRunningTest(stationId);
        if (!result.IsError) {
            var log = await this._testLogCollection.Find(e => e.StationId == stationId && e._id == result.Value)
                .FirstOrDefaultAsync();
            return log == null ? Error.NotFound() : log;
        } else {
            return Error.NotFound();
        }
    }
    
    public async Task<ErrorOr<BurnInTestLog>> StartNewUnknown(BurnInTestLog log,StationCurrent current) {
        var result = await this._stationDataService.GetTestConfiguration(current);
        if (result.IsError) {
            return Error.NotFound(description:$"Test configuration for {current.Value} not found");
        }
        log.SetCurrent=result.Value.SetCurrent;
        log.RunTime=result.Value.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        var logExists = await this.LogExists(log._id);
        if(logExists){
            var stationTaskResult=await this._stationDataService.SetRunningTest(log.StationId, log._id);
            if(stationTaskResult.IsError){
                var deleteResult=await this.DeleteTestLog(log._id);
                if(deleteResult.IsError){
                    return Error.Failure(description: "Failed to mark test as running.  Log was inserted, you must delete before trying again");
                }
                return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
            }
            return log;
        }
        return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
    }
    
    

    public async Task<ErrorOr<Deleted>> DeleteTestLog(ObjectId id) {
        var deleteResult = await this._testLogCollection.DeleteOneAsync(e => e._id == id);
        if (deleteResult.IsAcknowledged) {
            if(deleteResult.DeletedCount>0){
                return Result.Deleted;
            } else {
                return Error.Unexpected(description: "Log may not have been deleted");
            }
        } else {
            return Error.Failure(description: "Failed to delete log");
        }
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
            if(updateResult.ModifiedCount>0){
                return Result.Success;
            } else {
                return Error.Unexpected(description: "Unknown State. The test may not be marked as running");
            }
        } else {
            return Error.Failure(description: "Failed Mart Test as Running");
        }
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
        }else if (!clearResult.IsError && !logResult.IsAcknowledged) {
            return Error.Unexpected(description: "Unknown State. Log may not be finalized with Completed flag and StopTime");
        }else if (clearResult.IsError && logResult.IsAcknowledged) {
            return Error.Unexpected(description: "Unknown State. Log was finalize but station Running flag was not cleared");
        } else {
            return Error.Failure(description: "Station Running flag not cleared and Log was not finalized");
        }
    }
}
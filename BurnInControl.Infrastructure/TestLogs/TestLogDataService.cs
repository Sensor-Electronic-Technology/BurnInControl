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
    
    public async Task<ErrorOr<BurnInTestLog>> GetTestLog(string waferId) {
         var log=await this._testLogCollection.Find(e => e.TestSetup.Any(e => e.WaferId == waferId))
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
    
    //TODO: Should this delete the log if the station fails to be marked as running?
    public async Task<ErrorOr<Success>> StartNew(BurnInTestLog log) {
        log._id = ObjectId.GenerateNewId();
        var result = await this._stationDataService.GetTestConfiguration(log.SetCurrent);
        if (result.IsError) {
            return Error.NotFound(description:$"Test configuration for {log.SetCurrent.Name} not found");
        }
        log.RunTime=result.Value.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        var logExists = await this.LogExists(log._id);
        var stationTaskResult=await this._stationDataService.SetRunningTest(log.StationId, log._id);
        if(logExists && !stationTaskResult.IsError){
            return Result.Success;
        }else if (logExists && stationTaskResult.IsError) {
            //delete log and return error
            var deleteResult=await this.DeleteTestLog(log._id);
            if(deleteResult.IsError){
                return Error.Failure(description: "Failed to mark test as running.  Log was inserted, you must delete before trying again");
            } else {
                return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
            }
        } else if(!logExists && !stationTaskResult.IsError) {
            var clearRunningResult=await this._stationDataService.ClearRunningTest(log.StationId);
            if (clearRunningResult.IsError) {
                return Error.NotFound(description: "Error Not Found. Test was marked as running and failed to clear. Before trying again you must rest");
            } else {
                return Error.NotFound(description:"Log not found");
            }
        } else {
            return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
        }
    }
    
    public async Task<ErrorOr<Success>> StartNewUnknown(string stationId,StationCurrent current) {
        BurnInTestLog log=new BurnInTestLog();
        log._id = ObjectId.GenerateNewId();
        log.TestSetup.Add(new WaferSetup() {
            WaferId = "Unknown",
            Probe1 = StationProbe.Probe1,
            Probe2 = StationProbe.Probe2,
            StationPocket = StationPocket.LeftPocket
        });
        log.TestSetup.Add(new WaferSetup() {
            WaferId = "Unknown",
            Probe1 = StationProbe.Probe3,
            Probe2 = StationProbe.Probe4,
            StationPocket = StationPocket.MiddlePocket
        });
        log.TestSetup.Add(new WaferSetup() {
            WaferId = "Unknown",
            Probe1 = StationProbe.Probe5,
            Probe2 = StationProbe.Probe6,
            StationPocket = StationPocket.LeftPocket
        });
        log.SetCurrent = current;
        log.StationId = stationId;
        log.StartTime= DateTime.Now;
        log.Completed = false;
        log.ElapsedTime = 0;
        var result = await this._stationDataService.GetTestConfiguration(log.SetCurrent);
        if (result.IsError) {
            return Error.NotFound(description:$"Test configuration for {log.SetCurrent.Name} not found");
        }
        log.SetCurrent=result.Value.SetCurrent;
        log.RunTime=result.Value.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        var logExists = await this.LogExists(log._id);
        var stationTaskResult=await this._stationDataService.SetRunningTest(log.StationId, log._id);
        if(logExists && !stationTaskResult.IsError){
            return Result.Success;
        }else if (logExists && stationTaskResult.IsError) {
            //delete log and return error
            var deleteResult=await this.DeleteTestLog(log._id);
            if(deleteResult.IsError){
                return Error.Failure(description: "Failed to mark test as running.  Log was inserted, you must delete before trying again");
            } else {
                return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
            }
        } else if(!logExists && !stationTaskResult.IsError) {
            var clearRunningResult=await this._stationDataService.ClearRunningTest(log.StationId);
            if (clearRunningResult.IsError) {
                return Error.NotFound(description: "Error Not Found. Test was marked as running and failed to clear. Before trying again you must rest");
            } else {
                return Error.NotFound(description:"Log not found");
            }
        } else {
            return Error.Failure(description: "Failed to start new test. No data was modified, please try again");
        }
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

    public async Task<ErrorOr<BurnInTestLog>> CheckContinue(string stationId) {
        var result=await this._stationDataService.CheckForRunningTest(stationId);
        if(result.IsError) {
            return Error.NotFound(description:"Running Test Not Found");
        }
        var log=await this._testLogCollection.Find(e => e._id == result.Value)
            .FirstOrDefaultAsync();
        if(log==null){
            return Error.NotFound(description:"Running Test found burt log is missing");
        } else {
            return log;
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
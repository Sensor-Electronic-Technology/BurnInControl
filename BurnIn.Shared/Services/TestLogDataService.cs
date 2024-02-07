using BurnIn.Shared.AppSettings;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.StationData;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
namespace BurnIn.Shared.Services;

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
    
    public Task<BurnInTestLog?> GetTestLog(string waferId) {
        return this._testLogCollection.Find(e => e.TestSetup.Any(e => e.WaferId == waferId))
            .FirstOrDefaultAsync();
    }
    
    public Task<bool> LogExists(ObjectId id) {
        return this._testLogCollection.Find(e => e._id == id)
            .AnyAsync();
    }
    
    public async Task<bool> StartNew(BurnInTestLog log,StationCurrent current) {
        log._id = ObjectId.GenerateNewId();
        var testConfig = await this._stationDataService.GetTestConfiguration(current);
        if(testConfig == null) {
            return false;
        }
        log.SetCurrent=testConfig.SetCurrent;
        log.RunTime=testConfig.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        var logTask = this.LogExists(log._id);
        var stationTask=this._stationDataService.SetRunningTest(log.StationId, log._id);
        return await Task.WhenAll(logTask, stationTask).ContinueWith(e => e.Result.All(success => success));
    }

    public async Task<BurnInTestLog?> CheckContinue(string stationId) {
        var runningLogId=await this._stationDataService.CheckForRunningTest(stationId);
        if (runningLogId == null) {
            return null;
        }
        return await this._testLogCollection.Find(e => e._id == runningLogId)
            .FirstOrDefaultAsync();
    }
    
    public Task<bool> SetStart(ObjectId id,DateTime start,StationSerialData data) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var updateBuilder = Builders<BurnInTestLog>.Update;
        var update=updateBuilder.Set(e => e.StartTime,start)
            .Push(e => e.Readings,new StationReading() {
                TimeStamp = start,
                Data=data
            });
        return this._testLogCollection.UpdateOneAsync(filter,update)
            .ContinueWith(e => e.Result.IsAcknowledged);
    }
    
    public Task<bool> InsertReading(ObjectId id,StationSerialData data) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var updateBuilder = Builders<BurnInTestLog>.Update;
        var update=updateBuilder.Push(e => e.Readings,new StationReading() {
            TimeStamp = DateTime.Now,
            Data=data
        });
        return this._testLogCollection.UpdateOneAsync(filter,update)
            .ContinueWith(e => e.Result.IsAcknowledged);
    }
    
    public Task<bool> SetCompleted(ObjectId id,string stationId,DateTime stop) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var update = Builders<BurnInTestLog>.Update
            .Set(e => e.StopTime,stop)
            .Set(e=>e.Completed,true);
        var stationTask = this._stationDataService.ClearRunningTest(stationId);
        var logTask=this._testLogCollection.UpdateOneAsync(filter,update)
            .ContinueWith(e => e.Result.IsAcknowledged);
        return Task.WhenAll(logTask, stationTask)
            .ContinueWith(e => e.Result.All(success => success));
    }
}
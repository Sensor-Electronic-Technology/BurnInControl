using BurnIn.Shared.AppSettings;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Models.StationData;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
namespace BurnIn.Shared.Services;

public class StationDataService {
    
    private readonly IMongoCollection<Station> _stationCollection;
    private readonly IMongoCollection<TestConfiguration> _testConfigurationCollection;

    public StationDataService(IMongoClient client,IOptions<DatabaseSettings> settings) {
        var database = client.GetDatabase(settings.Value.DatabaseName?? "burn_in_db");
        this._stationCollection = database.GetCollection<Station>(settings.Value.StationCollectionName ?? "stations");
        this._testConfigurationCollection = database.GetCollection<TestConfiguration>(settings.Value.TestConfigCollectionName ?? "test_configurations");
    }
    
    public StationDataService(IMongoClient client) {
        var database = client.GetDatabase("burn_in_db");
        this._stationCollection = database.GetCollection<Station>("stations");
        this._testConfigurationCollection = database.GetCollection<TestConfiguration>("test_configurations");
    }
    
    public async Task<TestConfiguration?> GetTestConfiguration(StationCurrent setCurrent) {
        return await this._testConfigurationCollection
            .Find(e => e.SetCurrent == setCurrent)
            .FirstOrDefaultAsync();
    }
    
    public Task<bool> SetRunningTest(string stationId,ObjectId testLogId) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var update=updateBuilder.Set(e => e.RunningTest, testLogId)
            .Set(e => e.State, StationState.Running);
        return this._stationCollection.UpdateOneAsync(filter,update)
            .ContinueWith(e=>e.Result.IsAcknowledged);
    }
    
    public Task<ObjectId?> CheckForRunningTest(string stationId) {
        return this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.RunningTest)
            .FirstOrDefaultAsync();
    }
    
    public Task<bool> ClearRunningTest(string stationId) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var update=updateBuilder.Set(e => e.RunningTest, null)
            .Set(e => e.State, StationState.Running);
        return this._stationCollection.UpdateOneAsync(filter,update)
            .ContinueWith(e=>e.Result.IsAcknowledged);
    }
    
    public async Task<Result> InsertTestConfiguration(TestConfiguration config) {
        var exists=await this._testConfigurationCollection.Find(e => e.SetCurrent == config.SetCurrent)
            .AnyAsync();
        if(exists) {
            return ResultFactory.Error("Set Current configuration already exists.  There can only be one test per set current.");
        }
        await this._testConfigurationCollection.InsertOneAsync(config);
        return ResultFactory.Success();
    }

    public Task<BurnStationConfiguration?> GetConfiguration(string stationId) {
        return this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.Configuration)
            .FirstOrDefaultAsync();
    }

    public Task<string?> GetControllerFirmwareVersion(string stationId) {
        return this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.FirmwareVersion)
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> UpdateFirmwareVersion(string stationId,string version) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var result= await this._stationCollection
            .UpdateOneAsync(filter, updateBuilder.Set(e => e.FirmwareVersion, version));
        return result.IsAcknowledged;
    }
    
    public async Task<bool> SetUpdateAvailable(string stationId,bool isAvailable) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var result= await this._stationCollection
            .UpdateOneAsync(filter, updateBuilder.Set(e => e.UpdateAvailable, isAvailable));
        return result.IsAcknowledged;
    }
    
    public async Task<bool> CheckUpdateAvailable(string stationId) {
        var updateAvailable=await this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.UpdateAvailable)
            .FirstOrDefaultAsync();
        return updateAvailable;
    }

    public Task InsertStation(Station station) {
        return this._stationCollection.InsertOneAsync(station);
    }
    
    public async Task<bool> UpdateSubConfig<TConfig>(string stationId,TConfig config) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        switch (config) {
            case HeaterControllerConfig heatControlConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.HeaterConfig, heatControlConfig));
                return result.IsAcknowledged;
            }
            case ProbeControllerConfig probeControlConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.ProbesConfiguration, probeControlConfig));
                return result.IsAcknowledged;
            }
            case StationConfiguration stationConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.StationConfiguration, stationConfig));
                return result.IsAcknowledged;
            }
            default: {
                return false;
            }
        }
    }
}
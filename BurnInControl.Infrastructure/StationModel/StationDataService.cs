using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.AppSettings;
using ErrorOr;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
namespace BurnInControl.Infrastructure.StationModel;

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
    
    public async Task<ErrorOr<TestConfiguration>> GetTestConfiguration(StationCurrent setCurrent) {
        var testConfig=await this._testConfigurationCollection
            .Find(e => e.SetCurrent.Name == setCurrent.Name)
            .FirstOrDefaultAsync();
        if(testConfig==null) {
            return Error.NotFound();
        } else {
            return testConfig;
        }
    }
    
    public async Task<ErrorOr<Success>> SetRunningTest(string stationId,ObjectId testLogId) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var update=updateBuilder.Set(e => e.RunningTest, testLogId)
            .Set(e => e.State, StationState.Running);
         var success=await this._stationCollection.UpdateOneAsync(filter,update)
            .ContinueWith(e=>e.Result.IsAcknowledged);
         if(success) {
             return Result.Success;
         } else {
             return Error.Failure(description:"Failed to set running test");
         }
    }
    
    public async Task<ErrorOr<ObjectId>> CheckForRunningTest(string stationId) {
        var id=await this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.RunningTest)
            .FirstOrDefaultAsync();
        return id==null? Error.NotFound():id.Value;
    }
    
    public async Task<ErrorOr<Success>> ClearRunningTest(string stationId) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var update=updateBuilder.Set(e => e.RunningTest, null)
            .Set(e => e.State, StationState.Idle);
         var result=await this._stationCollection.UpdateOneAsync(filter, update);
         if (result.IsAcknowledged) {
             return Result.Success;
         } else {
             return Error.Failure(description:"Failed to clear running test");
         }
    }
    
    public async Task<ErrorOr<Success>> InsertTestConfiguration(TestConfiguration config) {
        var exists=await this._testConfigurationCollection.Find(e => e.SetCurrent == config.SetCurrent)
            .AnyAsync();
        if(exists) {
            return Error.Failure(description: "Test Configuration already exists. Only one test per set current is allowed.");
        }
        await this._testConfigurationCollection.InsertOneAsync(config);
        return Result.Success;
    }

    public Task<BurnStationConfiguration?> GetConfiguration(string stationId) {
        return this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.Configuration)
            .FirstOrDefaultAsync();
    }

    public async Task<ErrorOr<string>> GetControllerFirmwareVersion(string stationId) {
        var version=await this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.FirmwareVersion)
            .FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(version)) {
            return Error.NotFound(description: "Firmware Version Not Found");
        } else {
            return version;
        }
    }
    
    public async Task<ErrorOr<Success>> UpdateFirmwareVersion(string stationId,string version) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var result= await this._stationCollection
            .UpdateOneAsync(filter, updateBuilder.Set(e => e.FirmwareVersion, version));
        if (result.IsAcknowledged) {
            return Result.Success;
        } else {
            return Error.Failure(description:"Failed to update firmware version");
        }
    }
    
    public async Task<ErrorOr<string>> GetFirmwareVersion(string stationId) {
        var version=await this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.FirmwareVersion)
            .FirstOrDefaultAsync();
        if (!string.IsNullOrEmpty(version)) {
            return version;
        } else {
            return Error.Failure(description:"Version was null or empty,continue with update");
        }
    }
    
    public async Task<ErrorOr<Success>> SetUpdateAvailable(string stationId,bool isAvailable) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        var result= await this._stationCollection
            .UpdateOneAsync(filter, updateBuilder.Set(e => e.UpdateAvailable, isAvailable));
        if (result.IsAcknowledged) {
            return Result.Success;
        } else {
            return Error.Failure(description:"Failed to set update available");
        }
    }
    
    public async Task<ErrorOr<bool>> CheckUpdateAvailable(string stationId) {
        var updateAvailable=await this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.UpdateAvailable)
            .FirstOrDefaultAsync();
        return updateAvailable;
    }

    public Task InsertStation(Station station) {
        return this._stationCollection.InsertOneAsync(station);
    }
    
    public async Task<ErrorOr<Success>> UpdateSubConfig<TConfig>(string stationId,TConfig config) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        switch (config) {
            case HeaterControllerConfig heatControlConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.HeaterConfig, heatControlConfig));
                if (result.IsAcknowledged) {
                    return Result.Success;
                } else {
                    return Error.Failure(description:"Failed to update HeaterControllerConfig");
                }
            }
            case ProbeControllerConfig probeControlConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.ProbesConfiguration, probeControlConfig));
                if (result.IsAcknowledged) {
                    return Result.Success;
                } else {
                    return Error.Failure(description:"Failed to update ProbeControllerConfig");
                }
            }
            case StationConfiguration stationConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.StationConfiguration, stationConfig));
                if (result.IsAcknowledged) {
                    return Result.Success;
                } else {
                    return Error.Failure(description:"Failed to update StationConfiguration");
                }
            }
            default: {
                return Error.Unexpected(description:"Invalid Configuration Type");
            }
        }
    }
}
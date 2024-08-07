﻿using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.ComDefinitions;
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
    
    public async Task<IEnumerable<Station>> GetStations() {
        return await this._stationCollection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetStationList() {
        return await this._stationCollection.Find(_ => true).Project(e => e.StationId).ToListAsync();
    }

    public async Task<bool> SaveTuningResults(string stationId,List<HeaterTuneResult> tuningResults) {
        var heaterControllerConfig = await this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e=>e.Configuration.HeaterControllerConfig)
            .FirstOrDefaultAsync();
        if (heaterControllerConfig == null) {
            return false;
        }
        heaterControllerConfig.WindowSize = tuningResults[0].WindowSize;
        for (int i = 0; i < 3; i++) {
            heaterControllerConfig.HeaterConfigurations[i].PidConfig.Kp = tuningResults[i].kp;
            heaterControllerConfig.HeaterConfigurations[i].PidConfig.Ki = tuningResults[i].ki;
            heaterControllerConfig.HeaterConfigurations[i].PidConfig.Kd = tuningResults[i].kd;
        }
        var update=Builders<Station>.Update.Set(e => e.Configuration!.HeaterControllerConfig, heaterControllerConfig);
        var result=await this._stationCollection.UpdateOneAsync(e => e.StationId == stationId, update);
        return result.IsAcknowledged;
    }

    public async Task SetRunningTest(string stationId,ObjectId logId) {
        var update=Builders<Station>.Update.Set(e=>e.State,StationState.Running)
            .Set(e=>e.RunningTest,logId);
        await this._stationCollection.UpdateOneAsync(e=>e.StationId==stationId,update);
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


    public Task InsertStation(Station station) {
        return this._stationCollection.InsertOneAsync(station);
    }
    
    public async Task<ErrorOr<Success>> UpdateSubConfig<TConfig>(string stationId,TConfig config) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update;
        switch (config) {
            case HeaterControllerConfig heatControlConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.HeaterControllerConfig, heatControlConfig));
                if (result.IsAcknowledged) {
                    return Result.Success;
                } else {
                    return Error.Failure(description:"Failed to update HeaterControllerConfig");
                }
            }
            case ProbeControllerConfig probeControlConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.ProbeControllerConfig, probeControlConfig));
                if (result.IsAcknowledged) {
                    return Result.Success;
                } else {
                    return Error.Failure(description:"Failed to update ProbeControllerConfig");
                }
            }
            case StationConfiguration stationConfig: {
                var result=await this._stationCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.Configuration.ControllerConfig, stationConfig));
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
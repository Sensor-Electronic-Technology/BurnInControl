using BurnIn.Shared.Models.Configurations;
using MongoDB.Driver;
namespace BurnIn.ControlService.Services;

public class StationConfigurationService {
    private readonly IMongoDatabase _database;
    private IMongoCollection<BurnStationConfiguration> _stationConfigCollection;

    public StationConfigurationService(IMongoClient client) {
        this._database = client.GetDatabase("stress_stations");
        this._stationConfigCollection=this._database.GetCollection<BurnStationConfiguration>("station_configs");
    }

    public Task<BurnStationConfiguration> GetConfiguration(string stationId) {
        return this._stationConfigCollection
            .Find(e => e.StationId == stationId)
            .FirstOrDefaultAsync();
    }

    public Task InsertConfiguration(BurnStationConfiguration configuration) {
        return this._stationConfigCollection.InsertOneAsync(configuration);
    }

    /*public Task<UpdateResult> UpdateHeaterControllerConfig(string stationId,HeaterControllerConfig config) {
        var filter=Builders<BurnStationConfiguration>.Filter.Eq(e => e.StationId,stationId);
        var update = Builders<BurnStationConfiguration>.Update.Set(e => e.HeaterConfig, config);
        return this._stationConfigCollection.UpdateOneAsync(filter, update);
    }*/
    
    public async Task<bool> UpdateSubConfig<T>(string stationId,T config) {
        var filter=Builders<BurnStationConfiguration>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<BurnStationConfiguration>.Update;
        switch (config) {
            case HeaterControllerConfig heatControlConfig: {
                var result=await this._stationConfigCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.HeaterConfig, heatControlConfig));
                return result.IsAcknowledged;
            }
            case ProbeControllerConfig probeControlConfig: {
                var result=await this._stationConfigCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.ProbesConfiguration, probeControlConfig));
                return result.IsAcknowledged;
            }
            case StationConfiguration stationConfig: {
                var result=await this._stationConfigCollection
                    .UpdateOneAsync(filter, updateBuilder.Set(e => e.StationConfiguration, stationConfig));
                return result.IsAcknowledged;
            }
            default: {
                return false;
            }
        }
    }
}
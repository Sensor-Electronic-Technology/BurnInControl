using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Data.StationModel;
using BurnInControl.Shared.AppSettings;
using MongoDB.Driver;
using ErrorOr;
using Microsoft.Extensions.Options;
namespace BurnInControl.Infrastructure.StationModel;

public class StationConfigDataService {
    private readonly IMongoCollection<Station> _stationCollection;
    public StationConfigDataService(IMongoClient client,IOptions<DatabaseSettings> settings) {
        var database = client.GetDatabase(settings.Value.DatabaseName?? "burn_in_db");
        this._stationCollection = database.GetCollection<Station>(settings.Value.StationCollectionName ?? "stations");
    }
    public Task<BurnStationConfiguration?> GetStationBurnInConfig(string stationId) {
       return this._stationCollection.Find(e=>e.StationId==stationId).Project(e=>e.Configuration).FirstOrDefaultAsync();
    }

    public async Task<ulong> GetWindowSize(string stationId) {
        var windowSize = await this._stationCollection.Find(e => e.StationId == stationId)
            .Project(e => e.Configuration!.HeaterControllerConfig.WindowSize).FirstOrDefaultAsync();
        return windowSize;
    }

    public async Task<ErrorOr<Success>> UpdateAllConfig(string stationId, BurnStationConfiguration config) {
        var filter=Builders<Station>.Filter.Eq(e => e.StationId,stationId);
        var updateBuilder = Builders<Station>.Update.Set(station=>station.Configuration,config);
        var result=await this._stationCollection.UpdateOneAsync(filter, updateBuilder);
        if (result.IsAcknowledged) {
            return Result.Success;
        } else {
            return Error.Failure(description:"Failed to update Station Configuration");
        }
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
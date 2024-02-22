using BurnIn.Data.ComponentConfiguration.HeaterController;
using BurnIn.Data.ComponentConfiguration.ProbeController;
using BurnIn.Data.ComponentConfiguration.StationController;
using MongoDB.Driver;
namespace BurnInControl.Infrastructure.ComponentConfigurations;

public class ConfigDataService {
    private readonly IMongoCollection<HeaterControllerConfig> _heaterConfigCollection;
    private readonly IMongoCollection<ProbeControllerConfig> _probeConfigCollection;
    private readonly IMongoCollection<StationConfiguration> _stationConfigCollection;
    
    public ConfigDataService(IMongoClient client) {
        var database = client.GetDatabase("burn_in_db");
        this._heaterConfigCollection = database.GetCollection<HeaterControllerConfig>("heater_controller_config");
        this._probeConfigCollection = database.GetCollection<ProbeControllerConfig>("probe_controller_config");
        this._stationConfigCollection = database.GetCollection<StationConfiguration>("station_config");
    }
    

}
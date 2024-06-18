using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using MongoDB.Bson;
namespace BurnInControl.Data.StationModel;

public enum StationState {
    Running,Paused,TuningMode,Tuning,Heating,Idle,Offline
}

public class NetworkConfig {
    public string WifiIp { get; set; }
    public string EthernetIp { get; set; }
}

public class Station {
    public ObjectId _id { get; set; }
    public string StationId { get; set; }
    public string StationPosition { get; set; }
    public string? FirmwareVersion { get; set; }
    public bool UpdateAvailable { get; set; }
    public StationState State { get; set; }
    public ControllerSavedState? SavedState { get; set; }
    public ObjectId? RunningTest { get; set; }
    public NetworkConfig NetworkConfig { get; set; }
    public BurnStationConfiguration? Configuration { get; set; }
    
    public Station() {
        this.State = StationState.Idle;
        this.StationId = "Not Set";
        this.StationPosition="POS1";
        this.FirmwareVersion= "V.1.0";
        this.UpdateAvailable = false;
        this.RunningTest = null;
        this.SavedState= null;
        this.Configuration = new BurnStationConfiguration() {
            HeaterControllerConfig = new HeaterControllerConfig(),
            ProbeControllerConfig = new ProbeControllerConfig(),
            ControllerConfig = new StationConfiguration()
        };
    }
}


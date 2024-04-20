using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using MongoDB.Bson;
namespace BurnInControl.Data.StationModel;

public enum StationState {
    Running,Idle,Offline
}

public class Station {
    public ObjectId _id { get; set; }
    public ObjectId _Id { get; set; }
    public string StationId { get; set; }
    public string StationPosition { get; set; }
    public string? FirmwareVersion { get; set; }
    public bool UpdateAvailable { get; set; }
    public StationState State { get; set; }
    public ObjectId? RunningTest { get; set; }
    public BurnStationConfiguration? Configuration { get; set; }
    
    public Station() {
        this.State = StationState.Idle;
        this.StationId = "Not Set";
        this.StationPosition="POS1";
        this.FirmwareVersion= "V.1.0";
        this.UpdateAvailable = false;
        this.RunningTest = null;
        this.Configuration = new BurnStationConfiguration() {
            HeaterConfig = new HeaterControllerConfig(),
            ProbesConfiguration = new ProbeControllerConfig(),
            StationConfiguration = new StationConfiguration()
        };
    }
}


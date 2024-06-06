using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Data.ComponentConfiguration;

public class BurnStationConfiguration:IPacket,IBurnStationConfiguration{
    public HeaterControllerConfig HeaterControllerConfig { get; set; }
    public ProbeControllerConfig ProbeControllerConfig { get; set; }
    public StationConfiguration ControllerConfig { get; set; }
    
    
}
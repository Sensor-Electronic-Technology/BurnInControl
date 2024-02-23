using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Data.ComponentConfiguration;

public class BurnStationConfiguration:IPacket {
    public StationConfiguration StationConfiguration { get; set; }
    public HeaterControllerConfig HeaterConfig { get; set; }
    public ProbeControllerConfig ProbesConfiguration { get; set; }
}
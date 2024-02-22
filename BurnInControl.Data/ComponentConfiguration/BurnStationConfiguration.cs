using BurnIn.Data.ComDefinitions;
using BurnIn.Data.ComponentConfiguration.HeaterController;
using BurnIn.Data.ComponentConfiguration.ProbeController;
using BurnIn.Data.ComponentConfiguration.StationController;
namespace BurnIn.Data.Station.Configuration;

public class BurnStationConfiguration:IPacket {
    public StationConfiguration StationConfiguration { get; set; }
    public HeaterControllerConfig HeaterConfig { get; set; }
    public ProbeControllerConfig ProbesConfiguration { get; set; }
}
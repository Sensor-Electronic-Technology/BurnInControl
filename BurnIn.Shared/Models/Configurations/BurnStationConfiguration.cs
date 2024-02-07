using MongoDB.Bson;
namespace BurnIn.Shared.Models.Configurations;

public class BurnStationConfiguration:IPacket {
    public StationConfiguration StationConfiguration { get; set; }
    public HeaterControllerConfig HeaterConfig { get; set; }
    public ProbeControllerConfig ProbesConfiguration { get; set; }
}
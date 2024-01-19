using MongoDB.Bson;
namespace BurnIn.Shared.Models.Configurations;

public class BurnStationConfiguration {
    public ObjectId _id { get; set; }
    public string StationId { get; set; }
    public string StationPosition { get; set; }
    
    public StationConfiguration StationConfiguration { get; set; }
    public HeaterControllerConfig HeaterConfig { get; set; }
    public ProbeControllerConfig ProbesConfiguration { get; set; }
}
using BurnIn.Shared.Models.Configurations;
using MongoDB.Bson;
namespace BurnIn.Shared.Models.BurnInStationData;

public enum StationState {
    Running,Idle,Offline
}

public class Station {
    public ObjectId _Id { get; set; }
    public string? StationNumber { get; set; }
    public StationState State { get; set; }
    public BurnStationConfiguration? Configuration { get; set; }
}


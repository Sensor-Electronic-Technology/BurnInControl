using MongoDB.Bson;
namespace BurnIn.Shared.Models.BurnInStationData;

public class Station {
    public ObjectId _Id { get; set; }
    public int StationNumber { get; set; }
    public StationState State { get; set; }
    //public StationConfiguration? Configuration { get; set; }
}


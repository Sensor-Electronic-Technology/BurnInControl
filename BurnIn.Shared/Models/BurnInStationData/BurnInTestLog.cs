using MongoDB.Bson;
namespace BurnIn.Shared.Models.BurnInStationData;

public class BurnInTestLog {
    public ObjectId _id { get; set; }
    public ObjectId StationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public PocketLog? RightPocketLog { get; set; }
    public PocketLog? MiddlePocketLog { get; set; }
    public PocketLog? LeftPocketLog { get; set; }
}

public class PocketLog {
    public ProbeLog? Probe1Log { get; set; }
    public ProbeLog? Probe2Log { get; set; }
}

public class ProbeLog{
    public ProbeReading? InitialReading { get; set; }
    public ProbeReading? FinalReading { get; set; }
}






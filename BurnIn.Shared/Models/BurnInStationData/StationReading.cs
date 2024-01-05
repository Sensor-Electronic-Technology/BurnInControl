using MongoDB.Bson;
namespace BurnIn.Shared.Models.BurnInStationData;

public class StationReading {
    public ObjectId _id { get; set; }
    public ObjectId StationId { get; set; }
    public DateTime TimeStamp { get; set; }
    public long RunTime { get; set; }
    public long Elapsed { get; set; }
    public bool Running { get; set; }
    public bool Paused { get; set; }
    public Pocket RightPocket { get; set; }
    public Pocket MiddlePocket { get; set; }
    public Pocket LeftPocket { get; set; }
}

public class Pocket {
    public double Temperature { get; set; }
    public bool HeaterState { get; set; }
    public ProbeReading Probe1Reading { get; set; }
    public ProbeReading Probe2Reading { get; set; }
}

public class ProbeReading {
    public Pad Pad { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
    public long Runtime { get; set; }
}


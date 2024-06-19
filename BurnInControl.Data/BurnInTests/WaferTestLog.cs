using BurnInControl.Data.StationModel.Components;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QuickTest.Data.Models.Wafers.Enums;

namespace BurnInControl.Data.BurnInTests;

public class WaferTestLog {
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string WaferId { get; set; } = null!;
    public List<WaferTest> WaferTests { get; set; }
}

public class WaferTest {
    public ObjectId TestId { get; set; }
    public StationPocket Pocket { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public int BurnNumber { get; set; }
    public Dictionary<string, WaferPadData> WaferPadInitialData { get; set; } = new();
    public Dictionary<string, WaferPadData> WaferPadFinalData { get; set; } = new();
}

public class WaferPadData {
    public double Voltage { get; set; }
    public double Current { get; set; }
}

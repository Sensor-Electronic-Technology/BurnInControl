using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QuickTest.Data.Models.Wafers.Enums;

namespace BurnInControl.Data.BurnInTests;

public class WaferTestLog {
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string WaferId { get; set; } = null!;
    public int WaferSize { get; set; }
    public Dictionary<string, WaferPadData> WaferPadInitialData { get; set; } = new();
    public Dictionary<string, WaferPadData> WaferPadFinalData { get; set; } = new();
    public Dictionary<string,PocketData> PocketData { get; set; } = new();
    public List<WaferTest> WaferTests { get; set; } = new();
}

public class WaferTest {
    public ObjectId TestId { get; set; }
    public StationPocket Pocket { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public int BurnNumber { get; set; }
    public string? Probe1Pad { get; set; }
    public string? Probe2Pad { get; set; }
    public Dictionary<string, WaferPadData> WaferPadInitialData { get; set; } = new();
    public Dictionary<string, WaferPadData> WaferPadFinalData { get; set; } = new();
    public Dictionary<string,PocketData> PocketData { get; set; } = new();

    public void Set(PocketWaferSetup setup) {
        this.Probe1Pad = setup.Probe1Pad;
        this.Probe2Pad = setup.Probe2Pad;
        this.BurnNumber=setup.BurnNumber;
        this.Pocket = setup.StationPocket ?? StationPocket.LeftPocket;
    }

    public void Reset() {
        this.StartTime = DateTime.MinValue;
        this.StopTime = DateTime.MinValue;
        this.BurnNumber = 0;
        this.Probe1Pad = default;
        this.Probe2Pad = default;
        this.WaferPadInitialData.Clear();
        this.WaferPadFinalData.Clear();
    }
}

public class WaferPadData {
    public double Voltage { get; set; }
    public double Current { get; set; }
}

public class PocketData {
    public int Pocket { get; set; }
    public int SetCurrent { get; set; }
    public int SetTemperature { get; set; }
}

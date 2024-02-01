using MongoDB.Bson;
namespace BurnIn.Shared.Models.BurnInStationData;

public class BurnInTestLog {
    public ObjectId _id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }

    public List<WaferSetup> TestSetup { get; set; } = new List<WaferSetup>();
    public List<StationReading> Readings { get; set; } = new List<StationReading>();

    public void StartNew(List<WaferSetup> setup) {
        this.Clear();
        this._id = ObjectId.GenerateNewId();
        this.TestSetup = setup;
    }

    public void SetStart(DateTime start,StationSerialData data) {
        this.StartTime = start;
        this.Readings.Add(new StationReading() {
            TimeStamp = start,
            Data=data
        });
    }

    public void SetCompleted(DateTime stop) {
        this.StopTime = stop;
    }
    public void AddReading(StationSerialData data) {
        this.Readings.Add(new StationReading() {
            TimeStamp = DateTime.Now,
            Data=data
        });
    }

    public Result<IEnumerable<WaferResult>> GetReading(string waferId) {
        var waferSetup=this.TestSetup.FirstOrDefault(e => e.WaferId == waferId);
        if (waferSetup != null) {
            var waferResults=this.Readings.Select(e => new WaferResult() {
                TimeStamp = e.TimeStamp,
                Probe1RunTime = e.Data.ProbeRuntimes[waferSetup.Probe1.Value-1],
                Probe2RunTime = e.Data.ProbeRuntimes[waferSetup.Probe2.Value-1],
                Probe1Current = e.Data.Currents[waferSetup.Probe1.Value-1],
                Probe2Current = e.Data.Currents[waferSetup.Probe2.Value-1],
                Probe1Voltage = e.Data.Voltages[waferSetup.Probe1.Value-1],
                Probe2Voltage = e.Data.Voltages[waferSetup.Probe2.Value-1],
                PocketTemperature = e.Data.Temperatures[waferSetup.StationPocket.Value]
            });
            return ResultFactory.Success(waferResults);
        }
        return ResultFactory.Error(Enumerable.Empty<WaferResult>(), "Wafer not found");
    }
    public void Clear() {
        this.TestSetup.Clear();
        this.Readings.Clear();
        this._id = default;
        this.StartTime = default;
        this.StopTime = default;
    }
}

public class WaferSetup {
    public string WaferId { get; set; }
    public StationPocket StationPocket { get; set; }
    public StationProbe Probe1 { get; set; }
    public string? Probe1Pad { get; set; }
    public StationProbe Probe2 { get; set; }
    public string? Probe2Pad { get; set; }
}



public class WaferResult {
    public DateTime TimeStamp { get; set; }
    public int ElapsedSec { get; set; }
    public double Probe1Voltage { get; set; }
    public double Probe1Current { get; set; }
    public long Probe1RunTime { get; set; }
    
    public double Probe2Voltage { get; set; }
    public double Probe2Current { get; set; }
    public long Probe2RunTime { get; set; }
    public double PocketTemperature { get; set; }
}
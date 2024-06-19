using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions;
using ErrorOr;
using MongoDB.Bson;
namespace BurnInControl.Data.BurnInTests;

public class BurnInTestLogEntry {
    public ObjectId _id { get; set; }
    public ObjectId TestLogId { get; set; }
    public DateTime TimeStamp { get; set; }
    public StationSerialData? Reading { get; set; }
    public Dictionary<string, PocketLogEntry> PocketData { get; set; }
    
    public void SetReading(StationSerialData data) {
        this.Reading = data;
        this.PocketData = new Dictionary<string, PocketLogEntry>();
        foreach (var pocket in StationPocket.List) {
            PocketData.Add(pocket.Name,new PocketLogEntry() {
                Probe1Data = new ProbeLogData() {
                    Runtime = data.ProbeRuntimes[(pocket.Value-1)*2],
                    Voltage = data.Voltages[(pocket.Value-1)*2],
                    Current = data.Currents[(pocket.Value-1)*2],
                    Okay = data.ProbeRunTimeOkay[(pocket.Value-1)*2]
                },
                Probe2Data = new ProbeLogData() {
                    Runtime = data.ProbeRuntimes[((pocket.Value-1)*2)+1],
                    Voltage = data.Voltages[((pocket.Value-1)*2)+1],
                    Current = data.Currents[((pocket.Value-1)*2)+1],
                    Okay = data.ProbeRunTimeOkay[((pocket.Value-1)*2)+1]
                }
            });
        }
        /*PocketData.Add(StationPocket.MiddlePocket.Name,new PocketLogEntry() {
            Probe1Data = new ProbeLogData() {
                Runtime = data.ProbeRuntimes[2],
                Voltage = data.Voltages[2],
                Current = data.Currents[2],
                Okay = data.ProbeRunTimeOkay[2]
            },
            Probe2Data = new ProbeLogData() {
                Runtime = data.ProbeRuntimes[3],
                Voltage = data.Voltages[3],
                Current = data.Currents[3],
                Okay = data.ProbeRunTimeOkay[3]
            }
        });
        
        PocketData.Add(StationPocket.RightPocket.Name,new PocketLogEntry() {
            Probe1Data = new ProbeLogData() {
                Runtime = data.ProbeRuntimes[4],
                Voltage = data.Voltages[4],
                Current = data.Currents[4],
                Okay = data.ProbeRunTimeOkay[4]
            },
            Probe2Data = new ProbeLogData() {
                Runtime = data.ProbeRuntimes[5],
                Voltage = data.Voltages[5],
                Current = data.Currents[5],
                Okay = data.ProbeRunTimeOkay[5]
            }
        });*/
    }
}

public class ProbeLogData {
    public ulong Runtime { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
    public bool Okay { get; set; }
}

public class PocketLogEntry {
    public double Temperature { get; set; }
    public ProbeLogData Probe1Data { get; set; }
    public ProbeLogData Probe2Data { get; set; }
}

/*public class TestLogEntry {
    public ObjectId _id { get; set; }
    public ObjectId TestLogId { get; set; }
    public DateTime TimeStamp { get; set; }
    public StationSerialData? Reading { get; set; }

}*/

public class BurnInTestLog {
    public ObjectId _id { get; set; }
    public string StationId { get; set; }
    public StationCurrent? SetCurrent { get; set; }
    public int SetTemperature { get; set; }
    public long RunTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public bool Completed { get; set; }
    public long ElapsedTime { get; set; }
    
    //public List<WaferSetup> TestSetup { get; set; } = new List<WaferSetup>();
    public Dictionary<string,WaferSetup> TestSetup { get; set; } = new Dictionary<string,WaferSetup>();

    public void StartNew(List<WaferSetup> setup,int setTemp,StationCurrent current) {
        this.Reset();
        this.SetCurrent= current;
        this.SetTemperature = setTemp;
        this.TestSetup = new Dictionary<string, WaferSetup>();
        foreach(var pocket in StationPocket.List) {
            this.TestSetup.Add(pocket.Name,setup[pocket.Value-1]);
        }
    }

    public void SetStart(DateTime start,StationSerialData data) {
        this.StartTime = start;
    }
    
    public long RemainingTimeSecs() {
        return this.RunTime - this.ElapsedTime;
    }

    public void SetCompleted(DateTime stop) {
        this.StopTime = stop;
    }

    public void CreateUnknown(StationCurrent setCurrent,int setTemp,string stationId) {
        foreach(var pocket in StationPocket.List) {
            this.TestSetup.Add(pocket.Name,new WaferSetup() {
                WaferId = "Unknown",
                Probe1 = StationProbe.Probe1,
                Probe2 = StationProbe.Probe2,
                StationPocket = StationPocket.LeftPocket
            });
        }
        this.SetCurrent = setCurrent;
        this.SetTemperature= setTemp;
        this.StationId = stationId;
        this.StartTime= DateTime.Now;
        this.Completed = false;
        this.ElapsedTime = 0;
    }
    public void Reset() {
        this.TestSetup.Clear();
        this._id = default;
        this.StartTime = DateTime.MinValue;
        this.StopTime = DateTime.MinValue;
        this.ElapsedTime = 0;
        this.RunTime = 0;
        this.Completed = false;
        this.SetCurrent = StationCurrent._150mA;
        this.SetTemperature = 0;
    }
}
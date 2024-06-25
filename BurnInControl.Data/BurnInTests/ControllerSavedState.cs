using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using MongoDB.Bson;

namespace BurnInControl.Data.BurnInTests;

public class TimerData {
    public bool Running { get; set; } = false;
    public bool Paused { get; set; } = false;
    public ulong ElapsedSecs { get; set; } = 0;
    public ulong[] ProbeRunTimes { get; set; } = [];
    public ulong LastCheck { get; set; }= 0;
    public ulong DurationSecs { get; set; }= 0;
}

public class ControllerSavedState:IPacket {
    public StationCurrent SetCurrent { get; set; }
    public byte SetTemperature { get; set; }
    public TimerData CurrentTimes { get; set; } = new TimerData();
    public string TestId { get; set; }
    
    public ControllerSavedState() {
        //for json serialization
    }
    
    public ControllerSavedState(StationSerialData serialData) {
        this.CurrentTimes = new TimerData() {
            Running = serialData.Running,
            Paused = serialData.Paused,
            ElapsedSecs = serialData.ElapsedSeconds,
            ProbeRunTimes = serialData.ProbeRuntimes.ToArray(),
            LastCheck = serialData.RuntimeSeconds,
            DurationSecs = serialData.RuntimeSeconds
        };
        this.SetCurrent = StationCurrent.FromValue(serialData.CurrentSetPoint);
        this.SetTemperature= (byte)serialData.TemperatureSetPoint;
    }
}

public class SavedStateLog {
    public ObjectId? _id { get; set; } = default;
    public DateTime TimeStamp { get; set; }
    public string StationId { get; set; }= "S99";
    public ObjectId? LogId { get; set; } = default;
    public ControllerSavedState SavedState { get; set; } = new();
    
    public void Reset() {
        this._id = default;
        this.StationId= "S99";
        this.LogId = default;
        this.SavedState = default;
        SavedState = new();
    }
}
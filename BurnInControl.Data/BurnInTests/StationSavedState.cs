using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.MessagePacket;

namespace BurnInControl.Data.BurnInTests;

public class TimerData {
    public bool Running { get; set; }
    public bool Paused { get; set; }
    public ulong ElapsedSecs { get; set; }
    public ulong[] ProbeRunTimes { get; set; }
    public ulong LastCheck { get; set; }
    public ulong DurationSecs { get; set; }
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
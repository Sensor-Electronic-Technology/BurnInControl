using Ardalis.SmartEnum;
using MongoDB.Bson;

namespace BurnIn.Shared.Models.BurnInStationData;

public class HubCommandCarrier {
    public ArduinoCommand Command { get; set; }
}

public sealed class ArduinoCommand : SmartEnum<ArduinoCommand,string>
{
    public static readonly ArduinoCommand Start = new ArduinoCommand(nameof(Start), "S");
    public static readonly ArduinoCommand Pause = new ArduinoCommand(nameof(Pause),"P");
    public static readonly ArduinoCommand Reset = new ArduinoCommand(nameof(Reset), "RR");
    public static readonly ArduinoCommand HeaterToggle = new ArduinoCommand(nameof(HeaterToggle), "H");
    public static readonly ArduinoCommand Update = new ArduinoCommand(nameof(Update),"U");
    public static readonly ArduinoCommand CurrentToggle = new ArduinoCommand(nameof(CurrentToggle), "C");
    public static readonly ArduinoCommand Test = new ArduinoCommand(nameof(Test), "T");
    private ArduinoCommand(string name, string value) : base(name, value) {  }
}

public sealed class StationCurrent : SmartEnum<StationCurrent, int> {
    public static readonly StationCurrent _60mA = new StationCurrent("060", 60);
    public static readonly StationCurrent _120mA = new StationCurrent("120", 120);
    public static readonly StationCurrent _150mA = new StationCurrent("150", 150);

    public StationCurrent(string name, int value) : base(name, value) {}
}

public record RawReading {
    public double v11 { get; set; }
    public double v12{ get; set; }
    public double v21{ get; set; }
    public double v22{ get; set; }
    public double v31{ get; set; }
    public double v32{ get; set; }
    public double t1{ get; set; }
    public double t2{ get; set; }
    public double t3{ get; set; }
    public double i11{ get; set; }
    public double i12{ get; set; }
    public double i21{ get; set; }
    public double i22{ get; set; }
    public double i31{ get; set; }
    public double i32 { get; set; }
    public bool heating1{ get; set; }
    public bool heating2{ get; set; }
    public bool heating3{ get; set; }
    public bool running{ get; set; }
    public bool paused { get; set; }
    public double currentSP { get; set; }
    public double temperatureSP { get; set; }
    public long runTime { get; set; }
    public long elapsed { get; set; }
}

public record SerialData {
    public List<double> Voltages { get; set; }
    public List<double> Currents { get; set; }
    public List<double> Temperatures { get; set; }
    public List<long> ProbeRuntimes { get; set; }
    public bool Heater1State { get; set; }
    public bool Heater2State { get; set; }
    public bool Heater3State { get; set; }
    public int CurrentSetPoint { get; set; }
    public int TemperatureSetPoint { get; set; }
    public long RuntimeSeconds { get; set; }
    public long ElapsedSeconds { get; set; }
    public bool Running { get; set; }
    public bool Paused { get; set; }
}

public enum PadLocation {
    Center,
    Middle,
    Edge,
    TopMiddle,
    LeftMiddle,
    RightMiddle,
    BottomMiddle,
    TopEdge,
    LeftEdge,
    RightEdge,
    BottomEdge
}

public enum StationState {
    Running,Idle,Offline
}

public class Pad {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public PadLocation PadLocation { get; set; }
}



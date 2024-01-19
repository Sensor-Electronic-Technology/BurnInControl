using Ardalis.SmartEnum;
using MongoDB.Bson;

namespace BurnIn.Shared.Models.BurnInStationData;



public sealed class ArduinoCommand : SmartEnum<ArduinoCommand,int> {
    public static readonly ArduinoCommand Start = new ArduinoCommand(nameof(Start), 0);
    public static readonly ArduinoCommand Pause = new ArduinoCommand(nameof(Pause),1);
    public static readonly ArduinoCommand ToggleHeat = new ArduinoCommand(nameof(ToggleHeat), 2);
    public static readonly ArduinoCommand CycleCurrent = new ArduinoCommand(nameof(CycleCurrent), 3);
    public static readonly ArduinoCommand ProbeTest = new ArduinoCommand(nameof(ProbeTest), 4);
    public static readonly ArduinoCommand ChangeModeATune = new ArduinoCommand(nameof(ChangeModeATune), 5);
    public static readonly ArduinoCommand ChangeModeNormal = new ArduinoCommand(nameof(ChangeModeNormal), 6);
    public static readonly ArduinoCommand StartTune = new ArduinoCommand(nameof(StartTune), 7);
    public static readonly ArduinoCommand StopTune = new ArduinoCommand(nameof(StopTune), 8);
    public static readonly ArduinoCommand SaveATuneResult = new ArduinoCommand(nameof(SaveATuneResult), 9);
    public static readonly ArduinoCommand Reset = new ArduinoCommand(nameof(Reset), 10);
    
    private ArduinoCommand(string name, int value) : base(name, value) {  }
}

public sealed class ArduinoMsgPrefix : SmartEnum<ArduinoMsgPrefix,string> {
    public static readonly ArduinoMsgPrefix HeaterConfigPrefix= new ArduinoMsgPrefix(nameof(HeaterConfigPrefix), "CH");
    public static readonly ArduinoMsgPrefix ProbeConfigPrefix = new ArduinoMsgPrefix(nameof(ProbeConfigPrefix),"CP");
    public static readonly ArduinoMsgPrefix StationConfigPrefix = new ArduinoMsgPrefix(nameof(StationConfigPrefix), "CS");
    public static readonly ArduinoMsgPrefix SaveSate = new ArduinoMsgPrefix(nameof(HeaterResponse), "ST");
    public static readonly ArduinoMsgPrefix MessagePrefix = new ArduinoMsgPrefix(nameof(MessagePrefix), "M");
    public static readonly ArduinoMsgPrefix DataPrefix = new ArduinoMsgPrefix(nameof(DataPrefix), "D");
    public static readonly ArduinoMsgPrefix CommandPrefix = new ArduinoMsgPrefix(nameof(DataPrefix), "COM");
    public static readonly ArduinoMsgPrefix HeaterResponse = new ArduinoMsgPrefix(nameof(HeaterResponse), "HRES");
    public static readonly ArduinoMsgPrefix TestResponse = new ArduinoMsgPrefix(nameof(HeaterResponse), "TRES");
    public static readonly ArduinoMsgPrefix HeaterRequest = new ArduinoMsgPrefix(nameof(HeaterResponse), "HREQ");
    public static readonly ArduinoMsgPrefix TestRequest = new ArduinoMsgPrefix(nameof(HeaterResponse), "TREQ");
    public static readonly ArduinoMsgPrefix IdReceive = new ArduinoMsgPrefix(nameof(HeaterResponse), "IDREC");
    public static readonly ArduinoMsgPrefix IdRequest = new ArduinoMsgPrefix(nameof(HeaterResponse), "IDREQ");
    
    private ArduinoMsgPrefix(string name, string value) : base(name, value) {  }
}

public sealed class ArduinoResponse : SmartEnum<ArduinoResponse, int> {
    public static readonly ArduinoResponse HeaterSave = new ArduinoResponse(nameof(HeaterSave), 0);
    public static readonly ArduinoResponse HeaterCancel = new ArduinoResponse(nameof(HeaterSave), 1);
    public static readonly ArduinoResponse TestContinue = new ArduinoResponse(nameof(HeaterSave), 2);
    public static readonly ArduinoResponse TestCancel = new ArduinoResponse(nameof(HeaterSave), 3);
    private ArduinoResponse(string name,int value):base(name,value){}
}

public sealed class StationCurrent : SmartEnum<StationCurrent, int> {
    public static readonly StationCurrent _60mA = new StationCurrent("060", 60);
    public static readonly StationCurrent _120mA = new StationCurrent("120", 120);
    public static readonly StationCurrent _150mA = new StationCurrent("150", 150);

    private StationCurrent(string name, int value) : base(name, value) {}
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



using Ardalis.SmartEnum;
namespace BurnIn.Shared.Models.StationData;

public sealed class ArduinoResponse : SmartEnum<ArduinoResponse, int> {
    public static readonly ArduinoResponse HeaterSave = new ArduinoResponse(nameof(HeaterSave), 0);
    public static readonly ArduinoResponse HeaterCancel = new ArduinoResponse(nameof(HeaterSave), 1);
    public static readonly ArduinoResponse TestContinue = new ArduinoResponse(nameof(HeaterSave), 2);
    public static readonly ArduinoResponse TestCancel = new ArduinoResponse(nameof(HeaterSave), 3);
    private ArduinoResponse(string name,int value):base(name,value){}
}

public class StationPocket : SmartEnum<StationPocket, int> {
    public static StationPocket LeftPocket = new StationPocket(nameof(LeftPocket), 1);
    public static StationPocket MiddlePocket = new StationPocket(nameof(MiddlePocket), 2);
    public static StationPocket RightPocket = new StationPocket(nameof(RightPocket), 3);
    public StationPocket(string name, int value) : base(name, value) { }
}

public class StationProbe : SmartEnum<StationProbe, int> {
    public static StationProbe Probe1 = new StationProbe(nameof(Probe1), 1);
    public static StationProbe Probe2 = new StationProbe(nameof(Probe2), 2);
    public static StationProbe Probe3 = new StationProbe(nameof(Probe3), 3);
    public static StationProbe Probe4 = new StationProbe(nameof(Probe4), 4);
    public static StationProbe Probe5 = new StationProbe(nameof(Probe5), 5);
    public static StationProbe Probe6 = new StationProbe(nameof(Probe6), 6);
    public StationProbe(string name, int value) : base(name, value) {}
}








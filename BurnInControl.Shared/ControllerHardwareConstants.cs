namespace BurnInControl.Shared;

public static class ControllerHardwareConstants {
    public static readonly string[] HeaterNames = ["H1(L)", "H2(M)", "H3(R)"];
    public static readonly string[] ProbeNames = ["P1", "P2", "P3", "P4", "P5", "P6"];
    public static readonly int PROBE_COUNT = 6;
    public static readonly int HEATER_COUNT = 3;
    public static readonly int NTC_COUNT = 3;
    public static readonly int DIGITAL_COUNT = 53;
    public static readonly int ANALOG_COUNT = 16;
}
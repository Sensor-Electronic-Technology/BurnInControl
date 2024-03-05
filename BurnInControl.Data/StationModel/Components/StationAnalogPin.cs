using Ardalis.SmartEnum;
namespace BurnInControl.Data.StationModel.Components;

public class StationAnalogPin : SmartEnum<StationAnalogPin, sbyte> {
    public static StationAnalogPin A01 = new StationAnalogPin(nameof(A01), 54);
    public static StationAnalogPin A02 = new StationAnalogPin(nameof(A02), 55);
    public static StationAnalogPin A03 = new StationAnalogPin(nameof(A03), 56);
    public static StationAnalogPin A04 = new StationAnalogPin(nameof(A04), 57);
    public static StationAnalogPin A05 = new StationAnalogPin(nameof(A05), 58);
    public static StationAnalogPin A06 = new StationAnalogPin(nameof(A06), 59);
    public static StationAnalogPin A07 = new StationAnalogPin(nameof(A07), 60);
    public static StationAnalogPin A08 = new StationAnalogPin(nameof(A08), 61);
    public static StationAnalogPin A09 = new StationAnalogPin(nameof(A09), 62);
    public static StationAnalogPin A10 = new StationAnalogPin(nameof(A10), 63);
    public static StationAnalogPin A11 = new StationAnalogPin(nameof(A11), 64);
    public static StationAnalogPin A12 = new StationAnalogPin(nameof(A12), 65);
    public static StationAnalogPin A13 = new StationAnalogPin(nameof(A13), 66);
    public static StationAnalogPin A14 = new StationAnalogPin(nameof(A14), 67);
    public static StationAnalogPin A15 = new StationAnalogPin(nameof(A15), 68);
    public static StationAnalogPin A16 = new StationAnalogPin(nameof(A16), 69);
    
    public StationAnalogPin(String name, SByte value) : base(name, value) {
    }
}
using Ardalis.SmartEnum;

namespace BurnInControl.Data.StationModel.Components;

public class PadLocation : SmartEnum<PadLocation, string> {
    public static PadLocation PadLocationA = new PadLocation(nameof(PadLocationA), "a");
    public static PadLocation PadLocationB = new PadLocation(nameof(PadLocationB), "b");
    public static PadLocation PadLocationC = new PadLocation(nameof(PadLocationC), "c");
    public static PadLocation PadLocationD = new PadLocation(nameof(PadLocationD), "d");
    public static PadLocation PadLocationL = new PadLocation(nameof(PadLocationL), "l");
    public static PadLocation PadLocationR = new PadLocation(nameof(PadLocationR), "r");
    public static PadLocation PadLocationT = new PadLocation(nameof(PadLocationT), "t");
    public static PadLocation PadLocationG = new PadLocation(nameof(PadLocationG), "g");
    public PadLocation(String name, String value) : base(name, value) { }
}
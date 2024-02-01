
using Ardalis.SmartEnum; 
using MongoDB.Bson;
namespace BurnIn.Shared.Models.BurnInStationData;

/*public enum PadArea {
    a,
    b,
    c,
    d,
    l,
    r,
    t,
    g
}*/
public class PadLocation : SmartEnum<PadLocation, string> {
    public static PadLocation PadLocationA = new PadLocation(nameof(PadLocationA), "a");
    public static PadLocation PadLocationB = new PadLocation(nameof(PadLocationA), "b");
    public static PadLocation PadLocationC = new PadLocation(nameof(PadLocationA), "c");
    public static PadLocation PadLocationD = new PadLocation(nameof(PadLocationA), "d");
    public static PadLocation PadLocationL = new PadLocation(nameof(PadLocationA), "l");
    public static PadLocation PadLocationR = new PadLocation(nameof(PadLocationA), "r");
    public static PadLocation PadLocationT = new PadLocation(nameof(PadLocationA), "t");
    public static PadLocation PadLocationG = new PadLocation(nameof(PadLocationA), "g");
    public PadLocation(String name, String value) : base(name, value) { }
}

public class WaferArea : SmartEnum<WaferArea, string> {
    public static WaferArea Center = new WaferArea(nameof(Center), "C");
    public static WaferArea Middle = new WaferArea(nameof(Middle), "M");
    public static WaferArea Edge = new WaferArea(nameof(Edge), "E");

    public WaferArea(String name, String value) : base(name, value) { }
}

public class WaferPad {
    public ObjectId _id { get; set; }
    public PadLocation PadLocation { get; set; }
    public WaferArea WaferArea { get; set; }
    public int PadNumber { get; set; }
    public string? Identifier { get; set; }
}
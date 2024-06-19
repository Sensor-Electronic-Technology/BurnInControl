using BurnInControl.Data.StationModel.Components;
namespace BurnInControl.Data.BurnInTests.Wafers;

public class WaferSetup {
    public string WaferId { get; set; } = "Not Loaded";
    public int WaferSize { get; set; } = 2;
    public int BurnNumber { get; set; } = 0;
    public StationPocket? StationPocket { get; set; } = StationPocket.LeftPocket;
    public StationProbe? Probe1 { get; set; }=StationProbe.Probe1;
    public string? Probe1Pad { get; set; } = "";
    public StationProbe? Probe2 { get; set; }=StationProbe.Probe2;
    public string? Probe2Pad { get; set; }="";
    public bool Loaded { get; set; } = false;
}
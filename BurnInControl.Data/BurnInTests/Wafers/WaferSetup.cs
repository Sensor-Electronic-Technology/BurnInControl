using BurnInControl.Data.StationModel.Components;
namespace BurnInControl.Data.BurnInTests.Wafers;

public class WaferSetup {
    public string WaferId { get; set; }
    public int BurnNumber { get; set; }
    public StationPocket StationPocket { get; set; }
    public StationProbe Probe1 { get; set; }
    public string? Probe1Pad { get; set; }
    public StationProbe Probe2 { get; set; }
    public string? Probe2Pad { get; set; }
}
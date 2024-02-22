using Ardalis.SmartEnum;
namespace BurnIn.Data.StationModel.Components;

public class StationProbe : SmartEnum<StationProbe, int> {
    public static StationProbe Probe1 = new StationProbe(nameof(Probe1), 1);
    public static StationProbe Probe2 = new StationProbe(nameof(Probe2), 2);
    public static StationProbe Probe3 = new StationProbe(nameof(Probe3), 3);
    public static StationProbe Probe4 = new StationProbe(nameof(Probe4), 4);
    public static StationProbe Probe5 = new StationProbe(nameof(Probe5), 5);
    public static StationProbe Probe6 = new StationProbe(nameof(Probe6), 6);
    public StationProbe(string name, int value) : base(name, value) {}
}
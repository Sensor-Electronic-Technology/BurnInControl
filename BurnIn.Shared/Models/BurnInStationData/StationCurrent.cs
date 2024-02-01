using Ardalis.SmartEnum;
namespace BurnIn.Shared.Models.BurnInStationData;

public sealed class StationCurrent : SmartEnum<StationCurrent, int> {
    public static readonly StationCurrent _60mA = new StationCurrent("060", 60);
    public static readonly StationCurrent _120mA = new StationCurrent("120", 120);
    public static readonly StationCurrent _150mA = new StationCurrent("150", 150);

    private StationCurrent(string name, int value) : base(name, value) {}
}
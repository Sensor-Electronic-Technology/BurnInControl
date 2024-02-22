using Ardalis.SmartEnum;
namespace BurnIn.Data.StationModel.Components;

public class StationPocket : SmartEnum<StationPocket, int> {
    public static StationPocket LeftPocket = new StationPocket(nameof(LeftPocket), 1);
    public static StationPocket MiddlePocket = new StationPocket(nameof(MiddlePocket), 2);
    public static StationPocket RightPocket = new StationPocket(nameof(RightPocket), 3);
    public StationPocket(string name, int value) : base(name, value) { }
}
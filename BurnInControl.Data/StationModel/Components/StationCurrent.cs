using Ardalis.SmartEnum;
namespace BurnInControl.Data.StationModel.Components;

public class StationCurrent : SmartEnum<StationCurrent, int> {
    public static StationCurrent _60mA = new StationCurrent("60mA", 60);
    public static StationCurrent _120mA = new StationCurrent("120mA", 120);
    public static StationCurrent _150mA = new StationCurrent("150mA", 150);
    public StationCurrent(string name, int value) : base(name, value) {}
}
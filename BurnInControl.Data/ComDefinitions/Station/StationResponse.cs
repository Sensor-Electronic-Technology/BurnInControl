using Ardalis.SmartEnum;
namespace BurnIn.Data.ComDefinitions.Station;

public sealed class StationResponse : SmartEnum<StationResponse, int> {
    public static readonly StationResponse HeaterSave = new StationResponse(nameof(HeaterSave), 0);
    public static readonly StationResponse HeaterCancel = new StationResponse(nameof(HeaterSave), 1);
    public static readonly StationResponse TestContinue = new StationResponse(nameof(HeaterSave), 2);
    public static readonly StationResponse TestCancel = new StationResponse(nameof(HeaterSave), 3);
    private StationResponse(string name,int value):base(name,value){}
}
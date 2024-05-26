using BurnInControl.Shared.ComDefinitions;
namespace BurnInControl.Data.StationModel;

public class StationReading {
    public DateTime TimeStamp { get; set; }
    
    public StationSerialData Data { get; set; }
}
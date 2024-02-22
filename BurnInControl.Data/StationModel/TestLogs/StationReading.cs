using BurnIn.Data.ComDefinitions;
namespace BurnIn.Data.StationModel.TestLogs;

public class StationReading {
    public DateTime TimeStamp { get; set; }
    public StationSerialData Data { get; set; }
}
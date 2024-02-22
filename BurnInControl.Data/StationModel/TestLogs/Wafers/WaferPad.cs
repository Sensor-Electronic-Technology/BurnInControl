using MongoDB.Bson;
namespace BurnIn.Data.StationModel.TestLogs.Wafers;

public class WaferPad {
    public ObjectId _id { get; set; }
    public PadLocation PadLocation { get; set; }
    public WaferArea WaferArea { get; set; }
    public int PadNumber { get; set; }
    public string? Identifier { get; set; }
}
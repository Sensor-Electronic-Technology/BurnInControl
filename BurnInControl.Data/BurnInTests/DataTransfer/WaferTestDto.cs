using BurnInControl.Data.StationModel.Components;
using MongoDB.Bson;

namespace BurnInControl.Data.BurnInTests.DataTransfer;

public class WaferTestDto {
    public ObjectId TestId { get; set; }
    public string StationId { get; set; }
    public string Pocket { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public int BurnNumber { get; set; }
    public string? Probe1Pad { get; set; }
    public string? Probe2Pad { get; set; }
}
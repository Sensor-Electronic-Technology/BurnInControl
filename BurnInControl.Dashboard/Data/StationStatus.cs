using BurnInControl.Data.StationModel;

namespace BurnInControl.Dashboard.Data;

public class StationStatus {
    public string? StationId { get; set; }
    public StationState State { get; set; }
}
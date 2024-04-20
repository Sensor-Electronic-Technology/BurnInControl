using MongoDB.Bson;

namespace BurnInControl.Shared.FirmwareData;

public class VersionLog {
    public ObjectId _id { get; set; }
    public string Version { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public bool Latest { get; set; }
}

public class StationFirmwareTracker {
    public ObjectId _id { get; set; }
    public string StationId { get; set; }
    public string CurrentVersion { get; set; }
    public string AvailableVersion { get; set; }
    public bool UpdateAvailable { get; set; }
}
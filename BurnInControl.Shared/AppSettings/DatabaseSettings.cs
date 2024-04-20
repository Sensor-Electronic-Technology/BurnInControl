namespace BurnInControl.Shared.AppSettings;

public class DatabaseSettings {
    public string DatabaseName { get; set; }
    public string TestLogCollectionName { get; set; }
    public string StationCollectionName { get; set; }
    public string TestConfigCollectionName { get; set; }
    public string VersionCollectionName { get; set; }
    public string TrackerCollectionName { get; set; }
}
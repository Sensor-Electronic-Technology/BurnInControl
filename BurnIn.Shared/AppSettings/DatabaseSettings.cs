namespace BurnIn.Shared.AppSettings;

public class DatabaseSettings {
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string TestLogCollectionName { get; set; }
    public string StationCollectionName { get; set; }
    public string TestConfigCollectionName { get; set; }
}
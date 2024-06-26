﻿namespace BurnInControl.Shared.AppSettings;

public class DatabaseSettings {
    public string? DatabaseName { get; set; }
    public string? TestLogCollectionName { get; set; }
    public string? StationCollectionName { get; set; }
    public string? TestConfigCollectionName { get; set; }
    public string? VersionCollectionName { get; set; }
    public string? TrackerCollectionName { get; set; }
    public string? TestLogEntryCollection { get; set; }
    public string? WaferTestLogCollectionName { get; set; }
    public string? QuickTestEndpoint { get; set; }
    public string? QuickTestEndpointLocal { get; set; }
    
}
using MongoDB.Bson;
namespace BurnIn.Shared.Models.BurnInStationData;

public class Station {
    public ObjectId _Id { get; set; }
    public int StationNumber { get; set; }
    public StationState State { get; set; }
    public StationConfiguration? Configuration { get; set; }
}

public class StationConfiguration(StationCurrent defaultCurrent,
    int tempSetPoint,
    bool switchenabled,
    double timeOff,
    double currentPercent) {
    
     public StationCurrent DefaultCurrent { get; set; } 
     public int TemperatureSetPoint { get; set; } 
     public bool SwitchingEnabled { get; set; } 
     public double TimeOffPercent { get; set; } 
     public double CurrentPercent { get; set; } 

     public StationConfiguration() : this(StationCurrent._150mA,85,true,1.5,80.0) { }
}
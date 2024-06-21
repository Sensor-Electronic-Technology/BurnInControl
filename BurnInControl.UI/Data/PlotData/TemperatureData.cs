namespace BurnInControl.UI.Data;

public record TemperatureData {
    public double TimeSecs { get; set; }
    public double TempC { get; set; }
}

public record TemperatureLiveData {
    public double TimeHrs { get; set; }
    public double TempC { get; set; }
}
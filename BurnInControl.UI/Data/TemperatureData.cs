namespace BurnInControl.UI.Data;

public record TemperatureData {
    public double TimeSecs { get; set; }
    public double TempC { get; set; }
}
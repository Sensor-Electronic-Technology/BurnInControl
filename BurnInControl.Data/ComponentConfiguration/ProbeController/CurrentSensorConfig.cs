namespace BurnInControl.Data.ComponentConfiguration.ProbeController;

public class CurrentSensorConfig{
    public sbyte Pin { get; set; }
    public double fWeight { get; set; }
    public CurrentSensorConfig(){}
    public CurrentSensorConfig(sbyte pin = 0, double filter = 0) {
        this.Pin = pin;
        this.fWeight = filter;
    }
}
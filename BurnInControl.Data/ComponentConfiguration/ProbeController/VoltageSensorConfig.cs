namespace BurnIn.Data.ComponentConfiguration.ProbeController;

public class VoltageSensorConfig {
    public sbyte Pin { get; set; }
    public double fWeight { get; set; }
    public VoltageSensorConfig() { }
    public VoltageSensorConfig(sbyte pin = 0, double filter = 0) {
        this.Pin = pin;
        this.fWeight = filter;
    }
}
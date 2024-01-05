namespace BurnIn.Shared.Models.Configurations;

public class VoltageSensorConfig {
    public sbyte Pin { get; set; }
    public double fWeight { get; set; }
    public VoltageSensorConfig(sbyte pin = 0, double filter = 0) {
        this.Pin = pin;
        this.fWeight = filter;
    }
}

public class CurrentSensorConfig{
    public sbyte Pin { get; set; }
    public double fWeight { get; set; }
    public CurrentSensorConfig(sbyte pin = 0, double filter = 0) {
        this.Pin = pin;
        this.fWeight = filter;
    }
}

public class ProbeConfig {
    public VoltageSensorConfig VoltageSensorConfig { get; set; }
    public CurrentSensorConfig CurrentSensorConfig { get; set; }
    public ProbeConfig(VoltageSensorConfig voltConfig, CurrentSensorConfig currentConfig) {
        this.CurrentSensorConfig = currentConfig;
        this.VoltageSensorConfig = voltConfig;
    }
}

public class ProbeControllerConfig {
    public List<ProbeConfig> ProbeConfigurations { get; set; } = new List<ProbeConfig>();
    public ulong ReadInterval { get; set; }
}
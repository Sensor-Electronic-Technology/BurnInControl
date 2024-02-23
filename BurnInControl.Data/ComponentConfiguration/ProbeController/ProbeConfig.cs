namespace BurnInControl.Data.ComponentConfiguration.ProbeController;

public class ProbeConfig {
    public VoltageSensorConfig VoltageSensorConfig { get; set; }
    public CurrentSensorConfig CurrentSensorConfig { get; set; }
    public ProbeConfig(){}
    public ProbeConfig(VoltageSensorConfig voltConfig, CurrentSensorConfig currentConfig) {
        this.CurrentSensorConfig = currentConfig;
        this.VoltageSensorConfig = voltConfig;
    }
}
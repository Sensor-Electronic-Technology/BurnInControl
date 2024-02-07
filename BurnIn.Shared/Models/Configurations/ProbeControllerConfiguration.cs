namespace BurnIn.Shared.Models.Configurations;

public class CurrentSelectorConfig {
    public sbyte Pin120mA { get; set; }
    public sbyte Pin60mA { get; set; }
    public sbyte CurrentPin { get; set; }
    public int   SetCurrent { get; set; }
    public bool SwitchEnabled { get; set; }

    public CurrentSelectorConfig(sbyte pin, sbyte p120, sbyte p60, int current, bool enabled) {
        this.Pin120mA = p120;
        this.Pin60mA = p60;
        this.CurrentPin = pin;
        this.SetCurrent = current;
        this.SwitchEnabled = enabled;
    }
    
    public CurrentSelectorConfig(){}
}

public class VoltageSensorConfig {
    public sbyte Pin { get; set; }
    public double fWeight { get; set; }
    public VoltageSensorConfig() { }
    public VoltageSensorConfig(sbyte pin = 0, double filter = 0) {
        this.Pin = pin;
        this.fWeight = filter;
    }
}

public class CurrentSensorConfig{
    public sbyte Pin { get; set; }
    public double fWeight { get; set; }
    public CurrentSensorConfig(){}
    public CurrentSensorConfig(sbyte pin = 0, double filter = 0) {
        this.Pin = pin;
        this.fWeight = filter;
    }
}

public class ProbeConfig {
    public VoltageSensorConfig VoltageSensorConfig { get; set; }
    public CurrentSensorConfig CurrentSensorConfig { get; set; }
    public ProbeConfig(){}
    public ProbeConfig(VoltageSensorConfig voltConfig, CurrentSensorConfig currentConfig) {
        this.CurrentSensorConfig = currentConfig;
        this.VoltageSensorConfig = voltConfig;
    }
}

public class ProbeControllerConfig:IPacket {
    public CurrentSelectorConfig CurrentSelectConfig { get; set; }
    public List<ProbeConfig> ProbeConfigurations { get; set; } = new List<ProbeConfig>();
    public ulong ReadInterval { get; set; }
    public double CurrentPercent { get; set; }
    public int ProbeTestCurrent { get; set; }
    public ProbeControllerConfig() {
        
    }
}
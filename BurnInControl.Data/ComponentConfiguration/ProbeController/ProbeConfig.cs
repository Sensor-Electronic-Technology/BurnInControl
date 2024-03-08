using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
namespace BurnInControl.Data.ComponentConfiguration.ProbeController;

public class ProbeConfig {
    public int ProbeNumber { get; set; }
    public VoltageSensorConfig VoltageSensorConfig { get; set; }
    public CurrentSensorConfig CurrentSensorConfig { get; set; }
    public ProbeConfig(){}
    public ProbeConfig(int probeNumber,VoltageSensorConfig voltConfig, CurrentSensorConfig currentConfig) {
        this.ProbeNumber = probeNumber;
        this.CurrentSensorConfig = currentConfig;
        this.VoltageSensorConfig = voltConfig;
    }
}
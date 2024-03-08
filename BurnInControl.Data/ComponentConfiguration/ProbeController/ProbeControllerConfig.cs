using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Data.ComponentConfiguration.ProbeController;

public class ProbeControllerConfig:IPacket {

    public CurrentSelectorConfig CurrentSelectConfig { get; set; }
    public ProbeConfig[] ProbeConfigurations { get; set; }
    public ulong ReadInterval { get; set; }
    public double CurrentPercent { get; set; }
    public int ProbeTestCurrent { get; set; }
    public ProbeControllerConfig() {
        this.CurrentSelectConfig = new CurrentSelectorConfig(2, 6, 7, 60, true);
        this.CurrentPercent = .80;
        this.ProbeTestCurrent = 60;
        this.ReadInterval = 100;
        this.ProbeConfigurations = new[] {
            new ProbeConfig(1,new VoltageSensorConfig(54, 0.1), new CurrentSensorConfig(63, 0.1)),
            new ProbeConfig(2,new VoltageSensorConfig(55, 0.1), new CurrentSensorConfig(64, 0.1)),
            new ProbeConfig(3,new VoltageSensorConfig(56, 0.1), new CurrentSensorConfig(65, 0.1)),
            new ProbeConfig(4,new VoltageSensorConfig(57, 0.1), new CurrentSensorConfig(66, 0.1)),
            new ProbeConfig(5,new VoltageSensorConfig(58, 0.1), new CurrentSensorConfig(67, 0.1)),
            new ProbeConfig(6,new VoltageSensorConfig(59, 0.1), new CurrentSensorConfig(68, 0.1))
        };
    }
}
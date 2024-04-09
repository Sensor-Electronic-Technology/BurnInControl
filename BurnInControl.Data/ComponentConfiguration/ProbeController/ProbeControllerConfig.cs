using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Data.ComponentConfiguration.ProbeController;

public class ProbeControllerConfig:IPacket {

    public CurrentSelectorConfig CurrentSelectConfig { get; set; }
    public ProbeConfig[] ProbeConfigurations { get; set; }
    public ulong ReadInterval { get; set; }
    public double CurrentPercent { get; set; }
    public int ProbeTestCurrent { get; set; }
    public ProbeControllerConfig() {
        this.CurrentSelectConfig = new CurrentSelectorConfig(2, 6, 7, StationCurrent._150mA.Value, true);
        this.CurrentPercent = 93.5;
        this.ProbeTestCurrent = StationCurrent._60mA.Value;
        this.ReadInterval = 100;
        this.ProbeConfigurations = new[] {
            new ProbeConfig(1,new VoltageSensorConfig(StationAnalogPin.A01.Value, 0.1), new CurrentSensorConfig(StationAnalogPin.A10.Value, 0.1)),
            new ProbeConfig(2,new VoltageSensorConfig(StationAnalogPin.A02.Value, 0.1), new CurrentSensorConfig(StationAnalogPin.A11.Value, 0.1)),
            new ProbeConfig(3,new VoltageSensorConfig(StationAnalogPin.A03.Value, 0.1), new CurrentSensorConfig(StationAnalogPin.A12.Value, 0.1)),
            new ProbeConfig(4,new VoltageSensorConfig(StationAnalogPin.A04.Value, 0.1), new CurrentSensorConfig(StationAnalogPin.A13.Value, 0.1)),
            new ProbeConfig(5,new VoltageSensorConfig(StationAnalogPin.A05.Value, 0.1), new CurrentSensorConfig(StationAnalogPin.A14.Value, 0.1)),
            new ProbeConfig(6,new VoltageSensorConfig(StationAnalogPin.A06.Value, 0.1), new CurrentSensorConfig(StationAnalogPin.A15.Value, 0.1)),
            new ProbeConfig(6,new VoltageSensorConfig(StationAnalogPin.A06.Value, 0.1), new CurrentSensorConfig(StationAnalogPin.A15.Value, 0.1))
        };
    }
}
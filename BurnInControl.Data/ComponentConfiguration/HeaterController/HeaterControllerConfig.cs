﻿using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Data.ComponentConfiguration.HeaterController;

public class HeaterControllerConfig:IPacket,IBurnStationConfiguration {
    public List<HeaterConfiguration> HeaterConfigurations { get; set; }
    public ulong ReadInterval { get; set; }
    public int TemperatureSetPoint { get; set; }
    public ulong WindowSize { get; set; }
    public HeaterControllerConfig() {
        NtcConfiguration ntcConfig1 = new NtcConfiguration(1.159e-3f, 1.429e-4f, 1.118e-6f, StationAnalogPin.A06.Value, 0.01);
        NtcConfiguration ntcConfig2 = new NtcConfiguration(1.173e-3f, 1.736e-4f, 7.354e-7f, StationAnalogPin.A07.Value, 0.01);
        NtcConfiguration ntcConfig3 = new NtcConfiguration(1.200e-3f, 1.604e-4f, 8.502e-7f, StationAnalogPin.A08.Value, 0.01);

        PidConfiguration pidConfig1 = new PidConfiguration(51.42811,1.638761,1125.866);
        PidConfiguration pidConfig2 = new PidConfiguration(69.1832,2.294319,1408.117);
        PidConfiguration pidConfig3 = new PidConfiguration(36.24575,0.855281,1019.849);

        HeaterConfiguration heaterConfig1 = new HeaterConfiguration(ntcConfig1, pidConfig1, .1, 3, 1);
        HeaterConfiguration heaterConfig2 = new HeaterConfiguration(ntcConfig2, pidConfig2, .1, 4, 2);
        HeaterConfiguration heaterConfig3 = new HeaterConfiguration(ntcConfig3, pidConfig3, .1, 5, 3);
        
        this.HeaterConfigurations = new List<HeaterConfiguration> {
            heaterConfig1, 
            heaterConfig2, 
            heaterConfig3
        };
        this.ReadInterval = 100;
        this.TemperatureSetPoint = 85;
        this.WindowSize = 1000;
    }

    public HeaterControllerConfig Clone() {
        return (HeaterControllerConfig)this.MemberwiseClone();
    }
}

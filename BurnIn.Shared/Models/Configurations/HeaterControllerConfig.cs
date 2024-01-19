using BurnIn.Shared.Models.BurnInStationData;
using System.Text.Json;
namespace BurnIn.Shared.Models.Configurations;

public class NtcConfiguration {
    public double ACoeff { get; set; } = 0;
    public double BCoeff { get; set; } = 0;
    public double CCoeff { get; set; } = 0;
    public sbyte Pin { get; set; }
    public double fWeight { get; set; }
    public NtcConfiguration(){}
    public NtcConfiguration(double a, double b, double c, sbyte pin, double filter) {
        this.ACoeff = a;
        this.BCoeff = b;
        this.CCoeff = c;
        this.Pin = pin;
        this.fWeight = filter;
    }
}

public class PidConfiguration {
    public double Kp { get; set; }
    public double Ki { get; set; }
    public double Kd { get; set; }
    public ulong  WindowSizeMs { get; set; }
    public PidConfiguration(){}
    public PidConfiguration(double kp, double ki, double kd, ulong window) {
        this.Kp = kp;
        this.Ki = ki;
        this.Kd = kd;
        this.WindowSizeMs = window;
    }
}

public class HeaterConfiguration {
    public NtcConfiguration NtcConfig { get; set; }
    public PidConfiguration PidConfig { get; set; }
    public double TempDeviation { get; set; }
    public sbyte Pin { get; set; }
    public sbyte HeaterId { get; set; }
    public HeaterConfiguration(){}
    public HeaterConfiguration(NtcConfiguration ntcConfig, PidConfiguration pidConfig,
        double tempDev, sbyte pin, sbyte id) {
        this.NtcConfig = ntcConfig;
        this.PidConfig = pidConfig;
        this.TempDeviation = tempDev;
        this.Pin = pin;
        this.HeaterId = id;
    }
}

public class HeaterControllerConfig {
    public List<HeaterConfiguration> HeaterConfigurations { get; set; }
    public ulong ReadInterval { get; set; }
    public HeaterControllerConfig(){}
}

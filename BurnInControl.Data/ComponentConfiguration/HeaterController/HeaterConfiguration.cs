namespace BurnIn.Data.ComponentConfiguration.HeaterController;

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
namespace BurnIn.Data.ComponentConfiguration.HeaterController;

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
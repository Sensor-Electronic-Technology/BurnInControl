using BurnInControl.Data.StationModel.Components;
namespace BurnInControl.Data.ComponentConfiguration.HeaterController;

public class NtcConfiguration {
    public double ACoeff { get; set; }=1.45E-3;
    public double BCoeff { get; set; } = 0;
    public double CCoeff { get; set; } = 0;
    public sbyte Pin { get; set; } = StationAnalogPin.A01.Value;
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
namespace BurnInControl.Data.ComponentConfiguration.HeaterController;

public class PidConfiguration {
    public double Kp { get; set; }
    public double Ki { get; set; }
    public double Kd { get; set; }
    public PidConfiguration(){}
    public PidConfiguration(double kp, double ki, double kd) {
        this.Kp = kp;
        this.Ki = ki;
        this.Kd = kd;
    }

    public PidConfiguration Clone() {
        return MemberwiseClone() as PidConfiguration;
    }
}
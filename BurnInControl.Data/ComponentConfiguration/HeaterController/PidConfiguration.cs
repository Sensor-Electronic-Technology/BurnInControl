namespace BurnIn.Data.ComponentConfiguration.HeaterController;

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
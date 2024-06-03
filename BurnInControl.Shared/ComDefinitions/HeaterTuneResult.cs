namespace BurnInControl.Shared.ComDefinitions;

public class HeaterTuneResult {
    public int HeaterNumber { get; set; }
    public double kp { get; set; }
    public double ki { get; set; }
    public double kd { get; set; }
}
namespace BurnInControl.Data.BurnInTests.Wafers;

public class WaferResult {
    public DateTime TimeStamp { get; set; }
    public int ElapsedSec { get; set; }
    public double Probe1Voltage { get; set; }
    public double Probe1Current { get; set; }
    public ulong Probe1RunTime { get; set; }
    
    public double Probe2Voltage { get; set; }
    public double Probe2Current { get; set; }
    public ulong Probe2RunTime { get; set; }
    public double PocketTemperature { get; set; }
}
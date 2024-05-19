namespace BurnInControl.Data.DataTransfer;

public class PadResult {
    public string PadId { get; set; }
    public int Pocket { get; set; }
    public int SetTemperature { get; set; }
    public int SetCurrent { get; set; } 
    public double InitialVoltage { get; set; }
    public double InitialCurrent { get; set; }
    public double FinalVoltage { get; set; }
    public double FinalCurrent { get; set; }
}
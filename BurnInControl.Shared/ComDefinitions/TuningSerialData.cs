namespace BurnInControl.Shared.ComDefinitions;

public class TuningSerialData {
    public bool IsTuning { get; set; }
    public ulong ElapsedSeconds { get; set; }
    public float TemperatureSetPoint { get; set; }
    
    public List<double> Temperatures { get; set; } = [0, 0, 0];
    public List<bool> HeaterStates { get; set; } = [false, false, false];
    
    public TuningSerialData() {
        //for json serialization
    }
}
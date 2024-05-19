namespace BurnInControl.Shared.ComDefinitions;

public class StationSerialData {
    public List<double> Voltages { get; set; } = new List<double>(){0,0,0,0,0,0};
    public List<double> Currents { get; set; } = new List<double>(){0,0,0,0,0,0};
    public List<double> Temperatures { get; set; } = new List<double>(){0,0,0};
    public List<ulong> ProbeRuntimes { get; set; } = new List<ulong>(){0,0,0,0,0,0};
    public List<bool> ProbeRunTimeOkay { get; set; } = new List<bool>(){false,false,false,false,false,false};
    public List<bool> HeaterStates { get; set; } = new List<bool>(){false,false,false};
    public int CurrentSetPoint { get; set; }
    public int TemperatureSetPoint { get; set; }
    public ulong RuntimeSeconds { get; set; }
    public ulong ElapsedSeconds { get; set; }
    public bool Running { get; set; }
    public bool Paused { get; set; }
    public StationSerialData() {
        //for json serialization
    }
}
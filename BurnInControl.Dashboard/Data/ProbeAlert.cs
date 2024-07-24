namespace BurnInControl.Dashboard.Data;

public class ProbeAlert {
    public string ProbeId { get; set; }
    public bool Latched { get; set; }
    public bool Okay { get; set; }
    public bool Acknowledged { get; set; }
    
    public ProbeAlert(int index) {
        this.ProbeId = $"P{index+1}";
        this.Acknowledged = false;
        this.Latched = false;
        this.Okay = true;
    }
}
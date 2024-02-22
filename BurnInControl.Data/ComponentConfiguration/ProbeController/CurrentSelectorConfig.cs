namespace BurnIn.Data.ComponentConfiguration.ProbeController;

public class CurrentSelectorConfig {
    public sbyte Pin120mA { get; set; }
    public sbyte Pin60mA { get; set; }
    public sbyte CurrentPin { get; set; }
    public int   SetCurrent { get; set; }
    public bool SwitchEnabled { get; set; }

    public CurrentSelectorConfig(sbyte pin, sbyte p120, sbyte p60, int current, bool enabled) {
        this.Pin120mA = p120;
        this.Pin60mA = p60;
        this.CurrentPin = pin;
        this.SetCurrent = current;
        this.SwitchEnabled = enabled;
    }
    
    public CurrentSelectorConfig(){}
}
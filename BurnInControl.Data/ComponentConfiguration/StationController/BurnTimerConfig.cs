namespace BurnInControl.Data.ComponentConfiguration.StationController;

public class BurnTimerConfig {
    public ulong Time60mASec { get; set; }
    public ulong Time120mASec { get; set; }
    public ulong Time150mASec { get; set; }
    public BurnTimerConfig(ulong t60, ulong t120, ulong t150) {
        this.Time60mASec = t60;
        this.Time120mASec = t120;
        this.Time150mASec = t150;
        
    }
    public BurnTimerConfig(){}
}
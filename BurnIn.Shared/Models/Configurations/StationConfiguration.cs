namespace BurnIn.Shared.Models.Configurations;

public class BurnTimerConfig {
    public ulong Time60mASecs { get; set; }
    public ulong Time120mASecs { get; set; }
    public ulong Time150mASecs { get; set; }
    public BurnTimerConfig(ulong t60, ulong t120, ulong t150) {
        this.Time60mASecs = t60;
        this.Time120mASecs = t120;
        this.Time150mASecs = t150;
        
    }
}

public class StationConfiguration {
    public BurnTimerConfig BurnTimerConfig { get; set; }
    public ulong ComInterval { get; set; }
    public ulong UpdateInterval { get; set; }
    public ulong LogInterval { get; set; }

    public StationConfiguration(ulong comInterval, ulong uInterval, ulong logInterval) {
        this.ComInterval = comInterval;
        this.UpdateInterval = uInterval;
        this.LogInterval = logInterval;
    }
}
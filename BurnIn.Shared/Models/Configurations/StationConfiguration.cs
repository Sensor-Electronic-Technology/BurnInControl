using BurnIn.Shared.Models.BurnInStationData;
namespace BurnIn.Shared.Models.Configurations;

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

public class StationConfiguration:IPacket {
    public BurnTimerConfig BurnTimerConfig { get; set; }
    public ulong ComInterval { get; set; }
    public ulong UpdateInterval { get; set; }
    public ulong LogInterval { get; set; }
    public ulong VersionInterval { get; set; }

    public StationConfiguration(ulong comInterval, ulong uInterval, ulong logInterval,ulong verInterval) {
        this.ComInterval = comInterval;
        this.UpdateInterval = uInterval;
        this.LogInterval = logInterval;
        this.VersionInterval = verInterval;
    }
    
    public StationConfiguration(){}
}
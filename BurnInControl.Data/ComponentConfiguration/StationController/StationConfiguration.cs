using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Data.ComponentConfiguration.StationController;

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

    public StationConfiguration() {
        this.BurnTimerConfig = new BurnTimerConfig();
        this.ComInterval = 1000;
        this.UpdateInterval = 500;
        this.LogInterval = 300000;
        this.VersionInterval=3600000;
    }

    public StationConfiguration Clone() {
        return (StationConfiguration)MemberwiseClone();
    }
}
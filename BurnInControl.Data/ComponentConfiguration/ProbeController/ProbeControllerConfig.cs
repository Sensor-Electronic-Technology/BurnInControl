using BurnIn.Data.ComDefinitions;
namespace BurnIn.Data.ComponentConfiguration.ProbeController;

public class ProbeControllerConfig:IPacket {
    public CurrentSelectorConfig CurrentSelectConfig { get; set; }
    public List<ProbeConfig> ProbeConfigurations { get; set; } = new List<ProbeConfig>();
    public ulong ReadInterval { get; set; }
    public double CurrentPercent { get; set; }
    public int ProbeTestCurrent { get; set; }
    public ProbeControllerConfig() {
        
    }
}
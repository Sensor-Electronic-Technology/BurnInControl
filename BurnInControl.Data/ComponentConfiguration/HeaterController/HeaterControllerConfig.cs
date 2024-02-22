using BurnIn.Data.ComDefinitions;
namespace BurnIn.Data.ComponentConfiguration.HeaterController;

public class HeaterControllerConfig:IPacket {
    public List<HeaterConfiguration> HeaterConfigurations { get; set; }
    public ulong ReadInterval { get; set; }
    public HeaterControllerConfig(){}
}

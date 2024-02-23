using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Data.ComponentConfiguration.HeaterController;

public class HeaterControllerConfig:IPacket {
    public List<HeaterConfiguration> HeaterConfigurations { get; set; }
    public ulong ReadInterval { get; set; }
    public HeaterControllerConfig(){}
}

using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;

namespace BurnInControl.Data.ComponentConfiguration;

public class ConfigPacket<TConfig>:IPacket where TConfig : IBurnStationConfiguration{
    public ConfigType ConfigType { get; set; }
    public TConfig? Configuration { get; set; }
}
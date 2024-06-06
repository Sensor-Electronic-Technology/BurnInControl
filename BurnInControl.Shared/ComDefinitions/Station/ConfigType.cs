using System.Text.Json.Serialization;
using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;

namespace BurnInControl.Shared.ComDefinitions.Station;

[JsonConverter(typeof(ConfigTypeJsonConverter))]
public class ConfigType : SmartEnum<ConfigType,int>,IPacket {
    public static readonly ConfigType HeaterControlConfig = new ConfigType(nameof(HeaterControlConfig), 0);
    public static readonly ConfigType ProbeControlConfig = new ConfigType(nameof(ProbeControlConfig),1);
    public static readonly ConfigType ControllerConfig = new ConfigType(nameof(ControllerConfig), 2);
    public static readonly ConfigType BurnStationConfig = new ConfigType(nameof(BurnStationConfig), 3);
    private ConfigType(string name, int value) : base(name, value) {  }
}
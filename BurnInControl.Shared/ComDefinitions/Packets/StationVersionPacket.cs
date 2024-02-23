using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.Packets;

[JsonConverter(typeof(StationVersionPacketJsonConverter))]
public class StationVersionPacket : IPacket {
    public string Version { get; set; }
}
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.Packets;

[JsonConverter(typeof(StationIdPacketJsonConverter))]
public class StationIdPacket : IPacket {
    public string StationId { get; set; }
}
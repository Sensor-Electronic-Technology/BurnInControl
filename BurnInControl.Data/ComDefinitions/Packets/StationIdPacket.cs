using BurnIn.Data.ComDefinitions.JsonConverters;
using System.Text.Json.Serialization;
namespace BurnIn.Data.ComDefinitions.Packets;

[JsonConverter(typeof(StationIdPacketJsonConverter))]
public class StationIdPacket : IPacket {
    public string StationId { get; set; }
}
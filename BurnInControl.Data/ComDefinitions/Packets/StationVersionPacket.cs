using BurnIn.Data.ComDefinitions.JsonConverters;
using System.Text.Json.Serialization;
namespace BurnIn.Data.ComDefinitions.Packets;

[JsonConverter(typeof(StationVersionPacketJsonConverter))]
public class StationVersionPacket : IPacket {
    public string Version { get; set; }
}
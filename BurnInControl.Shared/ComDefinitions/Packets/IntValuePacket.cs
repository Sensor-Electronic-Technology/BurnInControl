using System.Text.Json.Serialization;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;

namespace BurnInControl.Shared.ComDefinitions.Packets;

[JsonConverter(typeof(IntValuePacketJsonConverter))]
public class IntValuePacket:IPacket {
    public int Value { get; set; }
}
using System.Text.Json.Serialization;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;

namespace BurnInControl.Shared.ComDefinitions.Packets;

[JsonConverter(typeof(TestIdPacketJsonConverter))]
public class TestIdPacket : IPacket {
    public string TestId { get; set; }
}
using System.Text.Json;
using System.Text.Json.Serialization;
using BurnInControl.Shared.ComDefinitions.Packets;

namespace BurnInControl.Shared.ComDefinitions.JsonConverters;

public class IntValuePacketJsonConverter : JsonConverter<IntValuePacket> {
    public override IntValuePacket Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return new IntValuePacket() { Value = reader.GetInt32()! };
    }
    public override void Write(Utf8JsonWriter writer, IntValuePacket value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.Value);
    }
}
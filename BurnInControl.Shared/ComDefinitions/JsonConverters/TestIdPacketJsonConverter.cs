using System.Text.Json;
using System.Text.Json.Serialization;
using BurnInControl.Shared.ComDefinitions.Packets;

namespace BurnInControl.Shared.ComDefinitions.JsonConverters;

public class TestIdPacketJsonConverter : JsonConverter<TestIdPacket> {
    public override TestIdPacket Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return new TestIdPacket() { TestId = reader.GetString()! };
    }
    public override void Write(Utf8JsonWriter writer, TestIdPacket value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.TestId);
    }
}
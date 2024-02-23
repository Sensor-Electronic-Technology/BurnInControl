using BurnInControl.Shared.ComDefinitions.Packets;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.JsonConverters;

public class StationVersionPacketJsonConverter : JsonConverter<StationVersionPacket> {
    public override StationVersionPacket Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return new StationVersionPacket() { Version = reader.GetString()! };
    }
    public override void Write(Utf8JsonWriter writer, StationVersionPacket value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.Version);
    }
}
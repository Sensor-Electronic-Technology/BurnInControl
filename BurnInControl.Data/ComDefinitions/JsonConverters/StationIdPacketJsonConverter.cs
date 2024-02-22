using BurnIn.Data.ComDefinitions.Packets;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BurnIn.Data.ComDefinitions.JsonConverters;

public class StationIdPacketJsonConverter : JsonConverter<StationIdPacket> {
    public override StationIdPacket Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return new StationIdPacket() { StationId = reader.GetString()! };
    }
    public override void Write(Utf8JsonWriter writer, StationIdPacket value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.StationId);
    }
}
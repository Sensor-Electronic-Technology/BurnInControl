using BurnIn.Data.ComDefinitions.Station;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BurnIn.Data.ComDefinitions.JsonConverters;

public class StationMsgPrefixJsonConverter : JsonConverter<StationMsgPrefix> {
    public override StationMsgPrefix Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return StationMsgPrefix.FromValue(reader.GetString()!);
    }
    public override void Write(Utf8JsonWriter writer, StationMsgPrefix value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.Value);
    }
}
using BurnInControl.Shared.ComDefinitions.Station;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.JsonConverters;

public class StationCommandJsonConverter : JsonConverter<StationCommand> {
    public override StationCommand Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return StationCommand.FromValue(reader.GetInt32());
    }
    public override void Write(Utf8JsonWriter writer, StationCommand value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.Value);
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;
using BurnInControl.Shared.ComDefinitions.Station;

namespace BurnInControl.Shared.ComDefinitions.JsonConverters;

public class ConfigTypeJsonConverter : JsonConverter<ConfigType> {
    public override ConfigType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return ConfigType.FromValue(reader.GetInt32());
    }
    public override void Write(Utf8JsonWriter writer, ConfigType value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.Value);
    }
}
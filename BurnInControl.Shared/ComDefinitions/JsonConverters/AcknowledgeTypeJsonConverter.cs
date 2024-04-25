using System.Text.Json;
using System.Text.Json.Serialization;
using BurnInControl.Shared.ComDefinitions.Station;

namespace BurnInControl.Shared.ComDefinitions.JsonConverters;

public class AcknowledgeTypeJsonConverter:JsonConverter<AcknowledgeType> {
        public override AcknowledgeType Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) {
            return AcknowledgeType.FromValue(reader.GetInt32());
        }
        public override void Write(Utf8JsonWriter writer, AcknowledgeType value, JsonSerializerOptions options) {
            writer.WriteNumberValue(value.Value);
        }
}
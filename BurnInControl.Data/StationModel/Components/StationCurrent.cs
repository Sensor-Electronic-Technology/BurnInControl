using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;

namespace BurnInControl.Data.StationModel.Components;

[JsonConverter(typeof(StationCurrentJsonConverter))]
public class StationCurrent : SmartEnum<StationCurrent, int>,IPacket {
    public static StationCurrent _60mA = new StationCurrent("60mA", 60);
    public static StationCurrent _120mA = new StationCurrent("120mA", 120);
    public static StationCurrent _150mA = new StationCurrent("150mA", 150);
    public StationCurrent(string name, int value) : base(name, value) {}
}


public class StationCurrentJsonConverter : JsonConverter<StationCurrent> {
    public override StationCurrent Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return StationCurrent.FromValue(reader.GetInt32());
    }
    public override void Write(Utf8JsonWriter writer, StationCurrent value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.Value);
    }
}
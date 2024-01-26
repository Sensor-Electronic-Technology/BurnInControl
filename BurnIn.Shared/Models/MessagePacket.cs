using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BurnIn.Shared.Models;


public class MessagePacket {
    public ArduinoMsgPrefix Prefix { get; set; }
    public object Packet { get; set; }
}

public interface IPacket { }

public class MessagePacketV2<TPacket> where TPacket:IPacket{
    public ArduinoMsgPrefix  Prefix { get; set; }
    public TPacket? Packet { get; set; }
}

[JsonConverter(typeof(StationIdPacketJsonConverter))]
public class StationIdPacket : IPacket {
    public string StationId { get; set; }
}

[JsonConverter(typeof(StationVersionPacketJsonConverter))]
public class StationVersionPacket : IPacket {
    public string Version { get; set; }
}

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


/*public abstract class Serializable

public interface IMessagePacket {
    string Prefix { get; set; }
    Serializable Serialize();
}*/






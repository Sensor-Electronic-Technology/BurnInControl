using Ardalis.SmartEnum;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BurnIn.Shared.Models.BurnInStationData;

[JsonConverter(typeof(ArduinoMsgPrefixJsonConverter))]
public class ArduinoMsgPrefix : SmartEnum<ArduinoMsgPrefix,string>,IPacket {
    public static readonly ArduinoMsgPrefix HeaterConfigPrefix= new ArduinoMsgPrefix(nameof(HeaterConfigPrefix), "CH");
    public static readonly ArduinoMsgPrefix ProbeConfigPrefix = new ArduinoMsgPrefix(nameof(ProbeConfigPrefix),"CP");
    public static readonly ArduinoMsgPrefix StationConfigPrefix = new ArduinoMsgPrefix(nameof(StationConfigPrefix), "CS");
    public static readonly ArduinoMsgPrefix SaveState = new ArduinoMsgPrefix(nameof(SaveState), "ST");
    public static readonly ArduinoMsgPrefix MessagePrefix = new ArduinoMsgPrefix(nameof(MessagePrefix), "M");
    public static readonly ArduinoMsgPrefix DataPrefix = new ArduinoMsgPrefix(nameof(DataPrefix), "D");
    public static readonly ArduinoMsgPrefix CommandPrefix = new ArduinoMsgPrefix(nameof(CommandPrefix), "COM");
    public static readonly ArduinoMsgPrefix HeaterResponse = new ArduinoMsgPrefix(nameof(HeaterResponse), "HRES");
    public static readonly ArduinoMsgPrefix TestResponse = new ArduinoMsgPrefix(nameof(TestResponse), "TRES");
    public static readonly ArduinoMsgPrefix HeaterRequest = new ArduinoMsgPrefix(nameof(HeaterResponse), "HREQ");
    public static readonly ArduinoMsgPrefix TestRequest = new ArduinoMsgPrefix(nameof(TestResponse), "TREQ");
    public static readonly ArduinoMsgPrefix IdReceive = new ArduinoMsgPrefix(nameof(IdReceive), "IDREC");
    public static readonly ArduinoMsgPrefix IdRequest = new ArduinoMsgPrefix(nameof(IdRequest), "IDREQ");
    public static readonly ArduinoMsgPrefix VersionReceive = new ArduinoMsgPrefix(nameof(VersionReceive), "VERREC");
    public static readonly ArduinoMsgPrefix VersionRequest = new ArduinoMsgPrefix(nameof(VersionRequest), "VERREQ");
    public static readonly ArduinoMsgPrefix InitMessage = new ArduinoMsgPrefix(nameof(InitMessage), "INIT");
    public static readonly ArduinoMsgPrefix TestStatus = new ArduinoMsgPrefix(nameof(TestStatus), "TSTAT");
    private ArduinoMsgPrefix(string name, string value) : base(name, value) {  }
}

public class ArduinoMsgPrefixJsonConverter : JsonConverter<ArduinoMsgPrefix> {
    public override ArduinoMsgPrefix Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return ArduinoMsgPrefix.FromValue(reader.GetString()!);
    }
    public override void Write(Utf8JsonWriter writer, ArduinoMsgPrefix value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.Value);
    }
}
using Ardalis.SmartEnum;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BurnIn.Shared.Models.BurnInStationData;

[JsonConverter(typeof(ArduinoCommandJsonConverter))]
public class ArduinoCommand : SmartEnum<ArduinoCommand,int>,IPacket {
    public static readonly ArduinoCommand Start = new ArduinoCommand(nameof(Start), 0);
    public static readonly ArduinoCommand Pause = new ArduinoCommand(nameof(Pause),1);
    public static readonly ArduinoCommand ToggleHeat = new ArduinoCommand(nameof(ToggleHeat), 2);
    public static readonly ArduinoCommand CycleCurrent = new ArduinoCommand(nameof(CycleCurrent), 3);
    public static readonly ArduinoCommand ProbeTest = new ArduinoCommand(nameof(ProbeTest), 4);
    public static readonly ArduinoCommand ChangeModeATune = new ArduinoCommand(nameof(ChangeModeATune), 5);
    public static readonly ArduinoCommand ChangeModeNormal = new ArduinoCommand(nameof(ChangeModeNormal), 6);
    public static readonly ArduinoCommand StartTune = new ArduinoCommand(nameof(StartTune), 7);
    public static readonly ArduinoCommand StopTune = new ArduinoCommand(nameof(StopTune), 8);
    public static readonly ArduinoCommand SaveATuneResult = new ArduinoCommand(nameof(SaveATuneResult), 9);
    public static readonly ArduinoCommand Reset = new ArduinoCommand(nameof(Reset), 10);
    private ArduinoCommand(string name, int value) : base(name, value) {  }
}

public class ArduinoCommandJsonConverter : JsonConverter<ArduinoCommand> {
    public override ArduinoCommand Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return ArduinoCommand.FromValue(reader.GetInt32());
    }
    public override void Write(Utf8JsonWriter writer, ArduinoCommand value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.Value);
    }
}
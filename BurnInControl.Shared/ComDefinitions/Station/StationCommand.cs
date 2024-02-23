using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.Station;

[JsonConverter(typeof(StationCommandJsonConverter))]
public class StationCommand : SmartEnum<StationCommand,int>,IPacket {
    public static readonly StationCommand Start = new StationCommand(nameof(Start), 0);
    public static readonly StationCommand Pause = new StationCommand(nameof(Pause),1);
    public static readonly StationCommand ToggleHeat = new StationCommand(nameof(ToggleHeat), 2);
    public static readonly StationCommand CycleCurrent = new StationCommand(nameof(CycleCurrent), 3);
    public static readonly StationCommand ProbeTest = new StationCommand(nameof(ProbeTest), 4);
    public static readonly StationCommand ChangeModeATune = new StationCommand(nameof(ChangeModeATune), 5);
    public static readonly StationCommand ChangeModeNormal = new StationCommand(nameof(ChangeModeNormal), 6);
    public static readonly StationCommand StartTune = new StationCommand(nameof(StartTune), 7);
    public static readonly StationCommand StopTune = new StationCommand(nameof(StopTune), 8);
    public static readonly StationCommand SaveATuneResult = new StationCommand(nameof(SaveATuneResult), 9);
    public static readonly StationCommand Reset = new StationCommand(nameof(Reset), 10);
    private StationCommand(string name, int value) : base(name, value) {  }
}
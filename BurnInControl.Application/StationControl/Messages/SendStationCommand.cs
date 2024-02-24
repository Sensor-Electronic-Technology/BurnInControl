using BurnInControl.Shared.ComDefinitions.Station;
namespace BurnInControl.Application.StationControl.Messages;

public class SendStationCommand {
    public StationCommand Command { get; set; }
}
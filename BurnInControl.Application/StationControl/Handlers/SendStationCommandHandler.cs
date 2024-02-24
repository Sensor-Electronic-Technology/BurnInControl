using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
namespace BurnInControl.Application.StationControl.Handlers;

public class SendStationCommandHandler {
    public Task Handle(SendStationCommand command, IStationController controller) {
        return controller.Send(StationMsgPrefix.CommandPrefix, command.Command);
    }
}
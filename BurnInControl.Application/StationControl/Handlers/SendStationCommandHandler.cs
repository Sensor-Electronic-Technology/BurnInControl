using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
namespace BurnInControl.Application.StationControl.Handlers;

public class SendStationCommandHandler {
    public async Task<ErrorOr<Success>> Handle(SendStationCommand command, IStationController controller) {
        return await controller.Send(StationMsgPrefix.CommandPrefix, command.Command);
    }
}
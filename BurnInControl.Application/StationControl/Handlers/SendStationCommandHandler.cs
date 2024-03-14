using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.StationControl.Handlers;

public class SendStationCommandHandler:IRequestHandler<SendStationCommand,ErrorOr<Success>> {
    private readonly IStationController _controller;
    public SendStationCommandHandler(IStationController controller) {
        _controller = controller;
    }
    
    public async Task<ErrorOr<Success>> Handle(SendStationCommand command, CancellationToken cancellationToken) {
        return await this._controller.Send(StationMsgPrefix.CommandPrefix, command.Command);
    }
}
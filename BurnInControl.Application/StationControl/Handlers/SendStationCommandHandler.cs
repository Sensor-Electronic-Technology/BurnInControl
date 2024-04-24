using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.StationControl.Handlers;

public class SendStationCommandHandler:IRequestHandler<SendStationCommand> {
    private readonly IStationController _controller;
    IMediator _mediator;
    public SendStationCommandHandler(IStationController controller,IMediator mediator) {
        _controller = controller;
        this._mediator = mediator;
    }
    
    public Task Handle(SendStationCommand command, CancellationToken cancellationToken) {
        if (command.Command == StationCommand.Reset) {
            var hardStopTask=this._mediator.Send(new HardStopCommand());
            var commandTask=this._controller.Send(StationMsgPrefix.CommandPrefix, command.Command);
            return Task.WhenAll(hardStopTask,commandTask);
        }
        return this._controller.Send(StationMsgPrefix.CommandPrefix, command.Command);
    }
}
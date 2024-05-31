using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BurnInControl.Application.StationControl.Handlers;

public class SendStationCommandHandler:IRequestHandler<SendStationCommand> {
    private readonly IStationController _controller;
    private readonly IMediator _mediator;
    private readonly ILogger<SendStationCommandHandler> _logger;
    public SendStationCommandHandler(IStationController controller,IMediator mediator,ILogger<SendStationCommandHandler> logger) {
        _controller = controller;
        this._mediator = mediator;
        this._logger = logger;
    }
    
    public Task Handle(SendStationCommand command, CancellationToken cancellationToken) {
        if (command.Command == StationCommand.Reset) {
            var hardStopTask=this._mediator.Send(new HardStopCommand(), cancellationToken);
            var commandTask=this._controller.Send(StationMsgPrefix.CommandPrefix, command.Command);
            this._logger.LogInformation("Received reset, sent command");
            return Task.WhenAll(hardStopTask,commandTask);
        }
        this._logger.LogInformation("Sending {Command}",command.Command.Name);
        return this._controller.Send(StationMsgPrefix.CommandPrefix, command.Command);
    }
}
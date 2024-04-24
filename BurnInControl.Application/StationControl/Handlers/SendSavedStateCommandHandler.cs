using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class SendSavedStateCommandHandler:IRequestHandler<SendSavedStateCommand> {
    private readonly IStationController _controller;

    public SendSavedStateCommandHandler(IStationController controller) {
        this._controller = controller;
    }

    public Task Handle(SendSavedStateCommand request, CancellationToken cancellationToken) {
        return this._controller.Send(StationMsgPrefix.LoadState, request.SavedState);
    }
}
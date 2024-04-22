using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class SendAckHandler:IRequestHandler<SendAckCommand> {
    private IStationController _controller;
    public SendAckHandler(IStationController controller) {
        this._controller = controller;
    }
    
    public Task Handle(SendAckCommand request, CancellationToken cancellationToken) {
        return this._controller.Send(StationMsgPrefix.AcknowledgePrefix,request.AcknowledgeType);
    }
}
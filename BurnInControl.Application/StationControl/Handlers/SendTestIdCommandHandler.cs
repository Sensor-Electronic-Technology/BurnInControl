using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class SendTestIdCommandHandler(IStationController controller) : IRequestHandler<SendTestIdCommand> {
    public Task Handle(SendTestIdCommand request, CancellationToken cancellationToken) {
        return controller.Send(StationMsgPrefix.SendTestId, new TestIdPacket() {
            TestId = request.TestId
        });
    }
}


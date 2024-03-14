using BurnInControl.Application.ProcessSerial.Messages;
using MediatR;
namespace BurnInControl.Application.ProcessSerial.Handlers;

public interface IStationMessageHandler:IRequestHandler<StationMessage>{
    public Task Handle(StationMessage message,CancellationToken cancellationToken);
}
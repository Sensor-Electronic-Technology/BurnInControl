using BurnInControl.Application.ProcessSerial.Messages;
namespace BurnInControl.Application.ProcessSerial.Handlers;

public interface IStationMessageHandler {
    public Task Handle(StationMessage message,CancellationToken cancellationToken);
}
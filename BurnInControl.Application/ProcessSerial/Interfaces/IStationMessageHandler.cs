using BurnInControl.Application.ProcessSerial.Messages;
namespace BurnInControl.Application.ProcessSerial.Interfaces;

public interface IStationMessageHandler {
    public Task Handle(StationMessage message,CancellationToken cancellationToken);
}
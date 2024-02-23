using BurnInControl.Application.ProcessSerial.Messages;
namespace BurnInControl.Application.ProcessSerial.Handlers;

public interface ISerialMessageHandler {
    public ValueTask Handle(StationMessage message);
}
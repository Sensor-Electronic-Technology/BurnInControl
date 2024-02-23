using BurnInControl.Application.StationControl.Messages;
namespace BurnInControl.Application.StationControl.Handlers;

public interface ISendSerialCommandHandler {
    public ValueTask Handle(SendSerialCommand serialCommand);
}
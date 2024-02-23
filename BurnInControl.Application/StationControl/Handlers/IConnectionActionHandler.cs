using BurnInControl.Application.StationControl.Messages;
namespace BurnInControl.Application.StationControl.Handlers;

public interface IConnectionActionHandler {
    public ValueTask Handle(ConnectionAction action);
}
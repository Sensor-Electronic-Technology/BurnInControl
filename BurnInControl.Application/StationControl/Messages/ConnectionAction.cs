using ErrorOr;
using MediatR;
namespace BurnInControl.Application.StationControl.Messages;

public enum ConnectAction {
    Connect,
    Disconnect
}

public class ConnectionAction:IRequest<ErrorOr<Success>>{
    public ConnectAction Action { get; set; }
}
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using ErrorOr;
using Wolverine;
namespace BurnInControl.Application.StationControl.Handlers;

public class ConnectionActionHandler {
    public async Task<ErrorOr<Success>> Handle(ConnectionAction action,IStationController controller,IMessageContext messageContext) {
        if(action.Action== ConnectAction.Connect) {
            return await controller.ConnectUsb();
        } else {
            return await controller.Disconnect();
        }
    }
}
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.StationControl.Handlers;

public class ConnectionActionHandler:IRequestHandler<ConnectionAction,ErrorOr<Success>> {
    private readonly IStationController _controller;
    public ConnectionActionHandler(IStationController controller) {
        this._controller = controller;
    }
    /*public async Task<ErrorOr<Success>> Handle(ConnectionAction action,IStationController controller) {
        if(action.Action== ConnectAction.Connect) {
            return await controller.ConnectUsb();
        } else {
            return await controller.Disconnect();
        }
    }*/
    public async Task<ErrorOr<Success>> Handle(ConnectionAction action, CancellationToken cancellationToken) {
        if(action.Action== ConnectAction.Connect) {
            return await this._controller.ConnectUsb();
        } else {
            return await this._controller.Disconnect();
        }
    }
}
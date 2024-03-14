using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using MediatR;
namespace StationService.Infrastructure.Hub;

public class StationHub:Hub<IStationHub> {
    private readonly IMediator _mediator;
    public StationHub(IMediator mediator) {
        _mediator = mediator;
    }
    public async Task SendSerialCom(StationSerialData serialData) {
        await this.Clients.All.OnSerialCom(serialData);
    }
    
    public async Task SendSerialComMessage(string message) {
        await this.Clients.All.OnSerialComMessage(message);
    }
    
    public async Task UsbConnectionAction(ConnectionAction action) {
        await this._mediator.Send(action);
    }
    public async Task SendCommand(StationCommand command) {
        await this._mediator.Send(new SendStationCommand(){Command = command});
    }
    
}
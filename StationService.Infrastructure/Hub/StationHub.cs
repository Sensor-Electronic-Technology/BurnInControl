using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.HubDefinitions.HubTransports;
using Microsoft.AspNetCore.SignalR;
using MediatR;
namespace StationService.Infrastructure.Hub;

public class StationHub:Hub<IStationHub> {
    private readonly IMediator _mediator;
    public StationHub(IMediator mediator) {
        this._mediator = mediator;
    }
    public override Task OnConnectedAsync() {
        return this._mediator.Send(new RequestConnectionStatus());
    }
    public Task RequestUsbConnectionStatus() {
        return this._mediator.Send(new RequestConnectionStatus());
    }
    
    public async Task SendSerialCom(StationSerialData serialData) {
        await this.Clients.All.OnStationData(serialData);
    }
    
    public async Task SendSerialComMessage(string message) {
        await this.Clients.All.OnSerialComMessage(message);
    }
    public async Task ConnectUsb() {
        await this._mediator.Send(new ConnectionAction() {
            Action=ConnectAction.Connect
        });
    }
    public async Task DisconnectUsb() {
        await this._mediator.Send(new ConnectionAction(){Action=ConnectAction.Disconnect});
    }
    public async Task SendCommand(StationCommand command) {
        await this._mediator.Send(new SendStationCommand(){Command = command});
    }
    
    public Task SetupTest(TestSetupTransport transport) {
        return this._mediator.Send(new TestSetupCommand() { TestSetupTransport = transport });
    }
    
    public Task OnUsbConnectFailed(string message) {
        return this.Clients.All.OnUsbConnectFailed(message);
    }

    public Task OnUsbDisconnectFailed(string message) {
        return this.Clients.All.OnUsbDisconnectFailed(message);
    }
    public Task OnUsbDisconnect(string message) {
        return this.Clients.All.OnUsbDisconnect(message);
    }
    
}
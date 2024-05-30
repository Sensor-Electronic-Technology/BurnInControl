using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using Microsoft.AspNetCore.SignalR;

namespace BurnInControl.UI.Hub;

/*public class StationHubProxy:Hub<IStationHub> {
    public StationHubProxy() {
    }
    
    public override Task OnConnectedAsync() {
        //return this._mediator.Send(new RequestConnectionStatus());
    }
    public Task RequestUsbConnectionStatus() {
        //return this._mediator.Send(new RequestConnectionStatus());
    }
    
    public Task SendSerialCom(StationSerialData serialData) { 
        //return this.Clients.All.OnStationData(serialData);
    }

    public Task RequestConfig(ConfigType type) {
        return this._mediator.Send(new RequestConfigMessage(){ConfigType = type});
    }
    public Task SendSerialComMessage(string message) {
        return this.Clients.All.OnSerialComMessage(message);
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

    public Task SendConfiguration(HeaterControllerConfig configuration) {
        Console.WriteLine("Received configuration from client."); 
        return this._mediator.Send(new SendConfiguration() { Configuration = configuration });
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
   
}*/
using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.HubDefinitions.HubTransports;
using Microsoft.AspNetCore.SignalR;
using MediatR;
namespace StationService.Infrastructure.Hub;

public class StationHub:StationHubController {
    private readonly IMediator _mediator;
    public StationHub(IMediator mediator) {
        this._mediator = mediator;
    }
    public override async Task OnConnectedAsync() {
        await this._mediator.Send(new RequestConnectionStatus());
        await Task.Delay(500);
        await this._mediator.Send(new RequestRunningTestCommand());
    }
    public override Task RequestUsbConnectionStatus() {
        return this._mediator.Send(new RequestConnectionStatus());
    }
    
    public override Task SendSerialCom(StationSerialData serialData) { 
        return this.Clients.All.OnStationData(serialData);
    }

    public override Task RequestConfig(ConfigType type) {
        return this._mediator.Send(new RequestConfigMessage(){ConfigType = type});
    }
    public override Task SendSerialComMessage(int type,string message) {
        return this.Clients.All.OnSerialComMessage(type,message);
    }
    public override async Task ConnectUsb() {
        await this._mediator.Send(new ConnectionAction() {
            Action=ConnectAction.Connect
        });
    }
    public override async Task DisconnectUsb() {
        await this._mediator.Send(new ConnectionAction(){Action=ConnectAction.Disconnect});
    }
    public override async Task SendCommand(StationCommand command) {
        await this._mediator.Send(new SendStationCommand(){Command = command});
    }
    
    public override Task SetupTest(TestSetupTransport transport) {
        return this._mediator.Send(new TestSetupCommand() { TestSetupTransport = transport });
    }

    public override Task SendHeaterControlConfig(HeaterControllerConfig heaterControlConfig) {
        Console.WriteLine("Received heater configuration from client."); 
        return this._mediator.Send(new SendConfiguration() { Configuration = heaterControlConfig });
    }
    
    public override Task SendProbeControlConfig(ProbeControllerConfig probeControlConfig) {
        Console.WriteLine("Received probe configuration from client."); 
        return this._mediator.Send(new SendConfiguration() { Configuration = probeControlConfig });
    }
    
    public override Task SendStationConfig(StationConfiguration stationConfig) {
        Console.WriteLine("Received station configuration from client."); 
        return this._mediator.Send(new SendConfiguration() { Configuration = stationConfig });
    }
    
    public override Task UpdateCurrentAndTemp(int current, int temp) {
        return this._mediator.Send(new UpdateCurrentTempCommand() { Current = current, Temperature = temp });
    }
    
    public override Task OnUsbConnectFailed(string message) {
        return this.Clients.All.OnUsbConnectFailed(message);
    }
    
    public override Task OnUsbDisconnectFailed(string message) {
        return this.Clients.All.OnUsbDisconnectFailed(message);
    }
    
    public override Task OnUsbDisconnect(string message) {
        return this.Clients.All.OnUsbDisconnect(message);
    }
    
    public override Task SaveTuningResults(List<HeaterTuneResult> results) {
        return this._mediator.Send(new SaveTuningResultsCommand() { Results = results });
    }

    public override Task SendTuningWindowSize(int windowSize) {
        return this._mediator.Send(new SendTuningWindowSizeCommand() { WindowSize = windowSize });
    }
}
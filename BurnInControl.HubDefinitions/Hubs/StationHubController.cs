using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using Microsoft.AspNetCore.SignalR;

namespace BurnInControl.HubDefinitions.Hubs;

public abstract class StationHubController:Hub<IStationHub>{
    public abstract override Task OnConnectedAsync();
    public abstract Task RequestUsbConnectionStatus();
    public abstract Task SendSerialCom(StationSerialData serialData);
    public abstract Task RequestConfig(ConfigType type);
    public abstract Task SendSerialComMessage(int type, string message);
    public abstract Task ConnectUsb();
    public abstract Task DisconnectUsb();
    public abstract Task SendCommand(StationCommand command);
    public abstract Task SetupTest(TestSetupTransport transport);
    public abstract Task SendConfiguration(HeaterControllerConfig configuration);
    public abstract Task UpdateCurrentAndTemp(int current, int temp);
    public abstract Task OnUsbConnectFailed(string message);
    public abstract Task OnUsbDisconnectFailed(string message);
    public abstract Task OnUsbDisconnect(string message);
    public abstract Task SaveTuningResults(List<HeaterTuneResult> results);
    public abstract Task SendTuningWindowSize(int windowSize);
}
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.Shared.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
namespace BurnInControl.UI.Services;

public class StationHubConnection {
    private readonly HubConnection _hubConnection;
    private readonly ILogger<StationHubConnection> _logger;
    
    public delegate Task AsyncEventHandler<in TArg>(TArg arg);
    public delegate Task AsyncStatusEventHandler<in TArg>(bool stat,TArg arg);
    
    public event AsyncEventHandler<string>? OnStationStatusChanged;
    public event AsyncEventHandler<string>? OnUsbConnected;
    public event AsyncEventHandler<string>? OnUsbDisconnected;
    public event AsyncEventHandler<string>? OnUsbConnectionFailed;
    
    public event AsyncStatusEventHandler<string>? OnHubConnected;
    public event AsyncEventHandler<string>? OnHubDisconnected;
    
    public event AsyncEventHandler<string>? OnStationMessageReceived;
    public event AsyncEventHandler<StationSerialData>? OnStationDataReceived;
    public event AsyncEventHandler<string>? OnTestStatus;
    
    public bool Connected=> this._hubConnection.State == HubConnectionState.Connected;
    
    public StationHubConnection(ILogger<StationHubConnection> logger) {
        this._logger= logger;
        var hubAddress=Environment.GetEnvironmentVariable("StationHub");
        string addr=string.IsNullOrEmpty(hubAddress) ? HubConstants.HubAddress:hubAddress;
        /*this._hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5066/hubs/station")
            .Build();*/
        this._hubConnection = new HubConnectionBuilder()
            .WithUrl(addr)
            .Build();
        this._hubConnection.On<StationSerialData>(HubConstants.Events.OnSerialCom,(data)=>this.OnStationDataReceived?.Invoke(data));
        this._hubConnection.On<string>(HubConstants.Events.OnSerialComMessage,(message)=>this.OnStationMessageReceived?.Invoke(message));
        this._hubConnection.On<string>(HubConstants.Events.OnTestStatus,(status)=>this.OnTestStatus?.Invoke(status));
        this._hubConnection.On<string>(HubConstants.Events.OnUsbDisconnect,(message)=>this.OnUsbDisconnected?.Invoke(message));
        this._hubConnection.On<string>(HubConstants.Events.OnUsbConnectFailed,(message)=>this.OnUsbConnectionFailed?.Invoke(message));
        this._hubConnection.On<string>(HubConstants.Events.OnUsbConnect, (message)=>this.OnUsbConnected?.Invoke(message));
    }
    
    public async Task StartConnection() {
        if (!this.Connected) {
            try {
                await this._hubConnection.StartAsync();
                if(this.Connected)
                    this.OnHubConnected?.Invoke(true,"Station Service Connected");
            }  catch(Exception e) {
                string error = e.Message;
                if (e.InnerException != null) {
                    error+="\n"+e.InnerException.Message;
                }
                this._logger.LogError($"Hub Connection Failed, Exception: \n {error}");
                this.OnHubConnected?.Invoke(false,"Station Service failed to connect, contact administrator");
            }
            
        }
    }
    
    public async Task StopConnection() {
        if (this.Connected) {
            await this._hubConnection.StopAsync();
        }
    }
    public async Task SendStart() {
        if (this.Connected) {
            await this._hubConnection.InvokeAsync(HubConstants.Methods.SendCommand, 
            StationCommand.Start);
        }
    }
    
    public async Task SendReset() {
        if (this.Connected) {
            await this._hubConnection.InvokeAsync(HubConstants.Methods.SendCommand, 
            StationCommand.Reset);
        }
    }
    
}
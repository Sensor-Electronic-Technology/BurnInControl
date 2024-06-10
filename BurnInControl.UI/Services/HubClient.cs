using BurnInControl.HubDefinitions.Hubs;

namespace BurnInControl.UI.Services;
using System.Net;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
public class HubClient:IAsyncDisposable {
    public HubConnection HubConnection { get;}
    public bool IsConnected=>HubConnection.State==HubConnectionState.Connected;
    private bool _started;

    public HubClient(IConfiguration configuration) {
        string hubAddress = configuration["HubAddress"] ?? HubConstants.HubAddress;
        this.HubConnection = new HubConnectionBuilder()
            .WithUrl(hubAddress)
            .WithAutomaticReconnect()
            .Build();
        
        /*this.HubConnection.On<ReceiveRequestedConfigEventArgs>(HubConstants.Events.OnRequestConfigHandler, args => {
            ReceiveRequestedConfig?.Invoke(this, args);
        });
        
        this.HubConnection.On<ReceiveSaveStatusEventArgs>(HubConstants.Events.OnConfigSaveStatus, args => {
            ReceiveSaveStatus?.Invoke(this, args);
        });
        
        this.HubConnection.On<SerialComMessageEventArgs>(HubConstants.Events.OnSerialComMessage, args => {
            SerialComMessage?.Invoke(this, args);
        });*/
        
    }
    
    public async Task StartAsync(CancellationToken cancellation = default) {
        if (this._started) return;
        this._started = true;
        await this.HubConnection.StartAsync(cancellation);
    }
    
    public async Task StopAsync(CancellationToken cancellation = default) {
        await HubConnection.StopAsync(cancellation);
        this._started = false;
    }
    
    public event EventHandler<ReceiveSaveStatusEventArgs>? ReceiveSaveStatus;
    public event EventHandler<ReceiveRequestedConfigEventArgs>? ReceiveRequestedConfig;
    public event EventHandler<SerialComMessageEventArgs>? SerialComMessage;
    
    
    public ValueTask DisposeAsync() {
        throw new NotImplementedException();
    }
}

public class ReceiveRequestedConfigEventArgs : EventArgs {
    public bool Success { get; set; }
    public int ConfigType { get; set; }
    public string? JsonConfig { get; set; }
}

public class ReceiveSaveStatusEventArgs : EventArgs {
    public string Type { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}

public class SerialComMessageEventArgs : EventArgs {
    public int Type { get; set; }
    public string? Message { get; set; }
}
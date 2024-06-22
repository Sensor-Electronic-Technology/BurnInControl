using BurnInControl.HubDefinitions.Hubs;
namespace BurnInControl.UI.Services;
using Microsoft.AspNetCore.SignalR.Client;
public class HubClient:IAsyncDisposable {
    public HubConnection HubConnection { get;}
    public bool IsConnected=>HubConnection.State==HubConnectionState.Connected;
    private bool _started;

    public HubClient(IConfiguration configuration) {
        string hubAddress = configuration["StationHub"] ?? StationHubConstants.HubAddress;
        //string hubAddress = "http://localhost:5066/hubs/station";
        this.HubConnection = new HubConnectionBuilder()
            .WithUrl(hubAddress)
            .WithAutomaticReconnect()
            .Build();
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
        return this.HubConnection.DisposeAsync();
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
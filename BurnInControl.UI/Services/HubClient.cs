using BurnInControl.HubDefinitions.Hubs;
namespace BurnInControl.UI.Services;
using Microsoft.AspNetCore.SignalR.Client;
public class HubClient:IAsyncDisposable {
    private readonly ILogger<HubClient> _logger;
    public HubConnection StationHubConnection { get;}
    public HubConnection HostHubConnection { get;}
    public bool StationHubIsConnected=>StationHubConnection.State==HubConnectionState.Connected;
    public bool HostHubIsConnected=>HostHubConnection.State==HubConnectionState.Connected;
    private bool _started;
    

    public HubClient(IConfiguration configuration,ILogger<HubClient> logger) {
        //string hubAddress = configuration["StationHub"] ?? StationHubConstants.HubAddress;
        string hostHubAddress = configuration["HostHub"] ?? HostHubConstants.HostHubAddress;
        string hubAddress = "http://localhost:5066/hubs/station";
        this._logger = logger;
        this.StationHubConnection = new HubConnectionBuilder()
            .WithUrl(hubAddress)
            .WithAutomaticReconnect()
            .Build();
        this.HostHubConnection = new HubConnectionBuilder()
            .WithUrl(hostHubAddress)
            .WithAutomaticReconnect()
            .Build();
    }
    
    public async Task StartAsync(CancellationToken cancellation = default) {
        if (this._started) return;
        this._started = true;
        try {
            await this.StationHubConnection.StartAsync(cancellation);
        } catch (Exception e) {
            this._logger.LogError(e, "Failed to start StationHubConnection");
        }
        
        try {
            await this.HostHubConnection.StartAsync(cancellation);
        } catch (Exception e) {
            this._logger.LogError(e, "Failed to start HostHubConnection");
        }
    }
    
    public async Task StopAsync(CancellationToken cancellation = default) {
        await StationHubConnection.StopAsync(cancellation);
        await this.HostHubConnection.StopAsync(cancellation);
        this._started = false;
    }
    
    public event EventHandler<ReceiveSaveStatusEventArgs>? ReceiveSaveStatus;
    public event EventHandler<ReceiveRequestedConfigEventArgs>? ReceiveRequestedConfig;
    public event EventHandler<SerialComMessageEventArgs>? SerialComMessage;
    
    
    public async ValueTask DisposeAsync() {
        await this.StationHubConnection.DisposeAsync();
        await this.HostHubConnection.DisposeAsync();
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
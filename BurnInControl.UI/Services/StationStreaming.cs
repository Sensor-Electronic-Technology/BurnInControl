using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
namespace BurnInControl.UI.Services;

public class StationStreaming:BackgroundService,IAsyncDisposable {
    /*private readonly ILogger<AmmoniaHubService> _logger;
    private readonly IHubContext<StationHub, ISendTankWeightsCommand> _hubContext;*/
    private HubConnection? _hubConnection;

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        throw new NotImplementedException();
    }
    public ValueTask DisposeAsync() {
        throw new NotImplementedException();
    }
}
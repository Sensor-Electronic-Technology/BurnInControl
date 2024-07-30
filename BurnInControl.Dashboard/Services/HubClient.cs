using BurnInControl.HubDefinitions.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
namespace BurnInControl.Dashboard.Services;

public class HubClient {
    private readonly ILogger<HubClient> _logger;
    public Dictionary<string,HubConnection> StationHubConnections { get; } = new();

    public HubClient() {
        
    }
}
using Microsoft.AspNetCore.Components.Server.Circuits;
namespace BurnInControl.UI.Services;

public class TrackingCircuitHandler : CircuitHandler
{
    private HashSet<Circuit> circuits = new();
    private readonly ILogger<TrackingCircuitHandler> _logger;
    private readonly StationHubConnection _hubConnection;
    
    public TrackingCircuitHandler(ILogger<TrackingCircuitHandler> logger,StationHubConnection hubConnection) {
        this._logger = logger;
        this._hubConnection = hubConnection;
    }

    public override Task OnConnectionUpAsync(Circuit circuit,
        CancellationToken cancellationToken) {
        if (this.circuits.Count == 0) {
            circuits.Add(circuit);
            this._logger.LogInformation($"Circuit Connected. Count: {circuits.Count}");
        } else {
            this._logger.LogWarning("Denied connection. Only one connection is allowed.");
        }
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, 
        CancellationToken cancellationToken) {
        circuits.Remove(circuit);
        this._logger.LogInformation($"Circuit Removed Count: {circuits.Count}");
        return this._hubConnection.StopConnection();
    }

    public int ConnectedCircuits => circuits.Count;
}
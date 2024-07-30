using Microsoft.AspNetCore.Components.Server.Circuits;
namespace BurnInControl.Dashboard.Services;

public class CircuitHandlerService:CircuitHandler {
    private readonly VisitorTrackingService _visitorTrackingService;
    
    public CircuitHandlerService(VisitorTrackingService visitorTrackingService) {
        this._visitorTrackingService = visitorTrackingService;
    }
    
    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken) {
        await this._visitorTrackingService.AddVisitor(circuit.Id);
        Console.WriteLine($"Added: {circuit.Id}");
        //return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }
    
    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken) {
        await this._visitorTrackingService.RemoveVisitor(circuit.Id);
        Console.WriteLine($"Removed: {circuit.Id}");
        //return base.OnConnectionDownAsync(circuit, cancellationToken);
    }
}
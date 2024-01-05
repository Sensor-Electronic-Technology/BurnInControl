namespace HubServer;

public class HubService:IHostedService, IDisposable {
    private readonly HubController _hubController;

    public HubService(HubController hubController) {
        this._hubController = hubController;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        await this._hubController.Start();
    }
    public async Task StopAsync(CancellationToken cancellationToken) {
        await this._hubController.Stop();
    }
    
    public void Dispose() {
        
    }
}
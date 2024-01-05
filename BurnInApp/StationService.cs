using BurnIn.Shared.Controller;
namespace BurnInApp;

public class StationService:IHostedService,IDisposable {
    private readonly StationController _stationController;
    private readonly ILogger<StationService> _logger;

    public StationService(StationController stationController, ILoggerFactory loggerFactory) {
        this._logger = loggerFactory.CreateLogger<StationService>();
        this._stationController = stationController;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        this._stationController.Start();
        Console.WriteLine("Station Service Started");
        return Task.CompletedTask;
    }
    public async Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Station Service Started");
        await this._stationController.Disconnect();
    }
    public void Dispose() {
        
    }
}
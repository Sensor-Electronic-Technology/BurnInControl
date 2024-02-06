using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace BurnIn.Shared.Services;

public class StationService:IHostedService,IDisposable {
    private readonly StationController _stationController;
    private readonly ILogger<StationService> _logger;

    public StationService(StationController stationController, ILogger<StationService> logger) {
        this._logger = logger;
        this._stationController = stationController;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        return this._stationController.Start();
    }
    
    public async Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Station Service Started");
        var result=await this._stationController.Stop();
        if (result.IsSuccess) {
            this._logger.LogInformation("Service Stopped \n"+result.Message);
        } else {
            this._logger.LogCritical($"Internal Error \n {result.Message}");
        }
    }
    public void Dispose() {
        
    }
}
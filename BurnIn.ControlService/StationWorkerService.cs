using BurnIn.ControlService.Infrastructure.HostedServices;
using BurnIn.ControlService.Infrastructure.Services;
namespace BurnIn.ControlService;

public class StationWorkerService:IHostedService,IDisposable {
    private readonly StationController _stationController;
    private readonly ILogger<StationService> _logger;

    public StationWorkerService(StationController stationController, ILogger<StationService> logger) {
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
        this._stationController.Dispose();
    }
}
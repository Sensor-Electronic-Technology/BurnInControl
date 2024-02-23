using BurnInControl.StationService.StationControl;
using ErrorOr;
namespace BurnInControl.StationService;

public class StationWorkerService:IHostedService,IDisposable {
    private readonly StationController _stationController;
    private readonly ILogger<StationWorkerService> _logger;

    public StationWorkerService(StationController stationController, ILogger<StationWorkerService> logger) {
        this._logger = logger;
        this._stationController = stationController;
    }
    

    public Task StartAsync(CancellationToken cancellationToken) {
        return this._stationController.Start();
    }
    
    public async Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Station Service Started");
        var result=await this._stationController.Stop();
        if (!result.IsError) {
            this._logger.LogInformation("Service Stopped");
        } else {
            this._logger.LogCritical($"Internal Error \n {result.FirstError.Description}");
        }
    }
    public void Dispose() {
        this._stationController.Dispose();
    }
}
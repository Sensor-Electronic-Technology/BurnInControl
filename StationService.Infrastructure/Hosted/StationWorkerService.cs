using BurnInControl.Application.FirmwareUpdate.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StationService.Infrastructure.Hosted;

public class StationWorkerService:IHostedService,IDisposable {
    private readonly IStationController _stationController;
    private readonly ILogger<StationWorkerService> _logger;
    private readonly IMediator _mediator;

    public StationWorkerService(IStationController stationController,IMediator mediator,
        ILogger<StationWorkerService> logger) {
        this._logger = logger;
        this._mediator = mediator;
        this._stationController = stationController;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken) {
        var succes=await this._mediator.Send(new StartupTryUpdateFirmwareCommand(), cancellationToken);
        await this._stationController.Start();
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
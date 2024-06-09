using BurnInControl.Application.BurnInTest;
using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BurnInControl.Application.FirmwareUpdate.Handlers;

/*public class TryUpdateFirmwareCommandHandler:IRequestHandler<TryUpdateFirmwareCommand,bool>  {
    private readonly ILogger<TryUpdateFirmwareCommandHandler> _logger;
    private readonly IFirmwareUpdateService _firmwareUpdateService;
    private readonly ITestService _testService;
    private readonly IStationController _stationController;
    private readonly IScheduler _scheduler;
    
    public TryUpdateFirmwareCommandHandler(ILogger<TryUpdateFirmwareCommandHandler> logger, 
        ITestService testService,
        IFirmwareUpdateService firmwareUpdateService, IStationController stationController,
        IScheduler scheduler) {
        this._logger = logger;
        this._firmwareUpdateService = firmwareUpdateService;
        this._testService = testService;
        this._stationController = stationController;
        this._scheduler = scheduler;
    }
    
    public async Task<bool> Handle(TryUpdateFirmwareCommand request, CancellationToken cancellationToken) {
        this._logger.LogInformation("Checking for firmware update");
        var result=await this._firmwareUpdateService.CheckForUpdate();
        if (result.UpdateAvailable && !this._testService.IsRunning) {
            this._logger.LogInformation("Update available, disconnecting station and uploading firmware update");
            await this._stationController.Disconnect();
            await this._firmwareUpdateService.UploadFirmwareUpdate();
            await this._stationController.ConnectUsb();
            return true;
        } else {
            this._scheduler.Schedule<IFirmwareUpdateJob>()
                .Hourly();
            return false;
        }
    }
}*/
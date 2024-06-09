using BurnInControl.Application.BurnInTest;
using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.StationControl.Interfaces;
using Coravel.Invocable;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace StationService.Infrastructure.Firmware.Jobs;

public class FirmwareUpdateJob:IFirmwareUpdateJob {
    private readonly ILogger<FirmwareUpdateJob> _logger;
    private readonly IFirmwareUpdateService _firmwareUpdateService;
    private readonly ITestService _testService;
    private readonly IStationController _stationController;
    
    public FirmwareUpdateJob(ILogger<FirmwareUpdateJob> logger, ITestService testService,
        IFirmwareUpdateService firmwareUpdateService, IStationController stationController) {
        this._logger = logger;
        this._firmwareUpdateService = firmwareUpdateService;
        this._testService = testService;
        this._stationController = stationController;
    }
    public async Task Invoke() {
        this._logger.LogInformation("Checking for firmware update");
        var result=await this._firmwareUpdateService.CheckForUpdate();
        if (result.UpdateAvailable && !this._testService.IsRunning) {
            this._logger.LogInformation("Update available, disconnecting station and uploading firmware update");
            await this._stationController.Disconnect();
            await this._firmwareUpdateService.UploadFirmwareUpdate();
            await this._stationController.ConnectUsb();
        }
    }
}
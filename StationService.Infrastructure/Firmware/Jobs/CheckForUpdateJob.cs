using BurnInControl.Application.BurnInTest;
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using Coravel.Invocable;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Extensions.Logging;

namespace StationService.Infrastructure.Firmware.Jobs;

public class CheckForUpdateJob:IInvocable{
    private readonly ILogger<CheckForUpdateJob> _logger;
    private readonly IFirmwareUpdateService _firmwareUpdateService;
    public CheckForUpdateJob(ILogger<CheckForUpdateJob> logger,IFirmwareUpdateService firmwareUpdateService) {
        this._logger = logger;
        this._firmwareUpdateService = firmwareUpdateService;
    }
    public Task Invoke() {
        return this._firmwareUpdateService.GetLatestVersion();
    }
}
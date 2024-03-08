using BurnInControl.Application.FirmwareUpdate.Interfaces;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace StationService.Infrastructure.Firmware.Jobs;

public class CheckForUpdateJob:IInvocable{
    private readonly ILogger<CheckForUpdateJob> _logger;
    private readonly IFirmwareUpdateService _firmwareUpdateService;
    public CheckForUpdateJob(ILogger<CheckForUpdateJob> logger,
        IFirmwareUpdateService firmwareUpdateService) {
        this._logger = logger;
        this._firmwareUpdateService = firmwareUpdateService;
    }
    public async Task Invoke() {
        var result=await this._firmwareUpdateService.GetLatestVersion();
        if (!result.IsError) {
            this._logger.LogInformation($"Latest firmware version is {result.Value}");
        } else {
            this._logger.LogError($"Error getting latest firmware version: {result.FirstError.Description}");
        }
    }
}
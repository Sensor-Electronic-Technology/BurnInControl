using BurnInControl.Application.BurnInTest;
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.HubDefinitions.Hubs;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BurnInControl.Application.FirmwareUpdate.Handlers;

public class UpdateFirmwareCommandHandler:IRequestHandler<StartupTryUpdateFirmwareCommand,bool> {
    private readonly ILogger<UpdateFirmwareCommandHandler> _logger;
    private readonly IFirmwareUpdateService _firmwareUpdateService;
    
    public UpdateFirmwareCommandHandler(ILogger<UpdateFirmwareCommandHandler> logger, IFirmwareUpdateService firmwareUpdateService) {
        this._logger = logger;
        this._firmwareUpdateService = firmwareUpdateService;
    }
    public async Task<bool> Handle(StartupTryUpdateFirmwareCommand request, CancellationToken cancellationToken) {
        var result=await this._firmwareUpdateService.CheckForUpdate();
        if (result.UpdateAvailable) {
            this._logger.LogInformation("Update available, disconnecting station and uploading firmware update");
            await this._firmwareUpdateService.UploadFirmwareUpdate();
            return true;
        }
        return false;
    }
}
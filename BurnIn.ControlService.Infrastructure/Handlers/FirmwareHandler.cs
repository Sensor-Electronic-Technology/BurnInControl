using BurnIn.ControlService.Infrastructure.Commands;
using BurnIn.ControlService.Infrastructure.Services;
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Handlers;
public class CheckForUpdateHandler : IRequestHandler<GetLatestVersionCommand> {
    private readonly FirmwareUpdateService _firmwareService;

    public CheckForUpdateHandler(FirmwareUpdateService firmwareService) {
        this._firmwareService = firmwareService;
    }
    public Task Handle(GetLatestVersionCommand request, CancellationToken cancellationToken) {
        return this._firmwareService.GetLatestVersion();
    }
}

public class UpdateHandler : IRequestHandler<UpdateCommand,UpdateResponse> {
    private readonly FirmwareUpdateService _firmwareService;
    public UpdateHandler(FirmwareUpdateService firmwareService) {
        this._firmwareService = firmwareService;
    }
    public Task<UpdateResponse> Handle(UpdateCommand request, CancellationToken cancellationToken) {
        return this._firmwareService.UploadFirmwareUpdate();
    }
}

using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.FirmwareUpdate.Handlers;

public class UpdateFirmwareCommandHandler:IRequestHandler<UpdateFirmwareCommand, ErrorOr<(string ver, string avrOutput)>> {
    private IFirmwareUpdateService _firmwareUpdateService;
    public UpdateFirmwareCommandHandler(IFirmwareUpdateService firmwareUpdateService) {
        this._firmwareUpdateService = firmwareUpdateService;
    }
    public Task<ErrorOr<(String ver, String avrOutput)>> Handle(UpdateFirmwareCommand request, CancellationToken cancellationToken) {
        return this._firmwareUpdateService.UploadFirmwareUpdate();
    }
}
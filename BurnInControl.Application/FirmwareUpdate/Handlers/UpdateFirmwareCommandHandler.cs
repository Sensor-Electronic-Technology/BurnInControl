using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using ErrorOr;
namespace BurnInControl.Application.FirmwareUpdate.Handlers;

public class UpdateFirmwareCommandHandler {
    public async Task<ErrorOr<(string ver, string avrOutput)>> Handle(UpdateFirmwareCommand command, IFirmwareUpdateService service) {
        return await service.UploadFirmwareUpdate();
    }
}
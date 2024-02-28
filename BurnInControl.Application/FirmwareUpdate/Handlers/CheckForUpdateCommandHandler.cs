using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using ErrorOr;
namespace BurnInControl.Application.FirmwareUpdate.Handlers;

public class CheckForUpdateCommandHandler {
    public async Task<ErrorOr<string>> Handle(CheckForUpdateCommand command, IFirmwareUpdateService service) {
        return await service.GetLatestVersion();
    }
}
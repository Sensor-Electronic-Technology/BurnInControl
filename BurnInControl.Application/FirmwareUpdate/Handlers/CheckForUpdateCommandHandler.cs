using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using ErrorOr;
using MediatR;

namespace BurnInControl.Application.FirmwareUpdate.Handlers;

public class CheckForUpdateCommandHandler:IRequestHandler<CheckForUpdateCommand> {
    private IFirmwareUpdateService _updateService;
    
    public CheckForUpdateCommandHandler(IFirmwareUpdateService updateService) {
        this._updateService = updateService;
    }
    
    public Task Handle(CheckForUpdateCommand request, CancellationToken cancellationToken) {
        return this._updateService.GetLatestVersion();
    }
}
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using BurnInControl.HubDefinitions.Hubs;
using ErrorOr;
using MediatR;

namespace BurnInControl.Application.FirmwareUpdate.Handlers;

public class UpdateFirmwareCommandHandler:IRequestHandler<UpdateFirmwareCommand> {
    private IFirmwareUpdateService _firmwareUpdateService;
    
    public UpdateFirmwareCommandHandler(IFirmwareUpdateService firmwareUpdateService) {
        this._firmwareUpdateService = firmwareUpdateService;
    }
    public async Task Handle(UpdateFirmwareCommand request, CancellationToken cancellationToken) {
        
    }
}
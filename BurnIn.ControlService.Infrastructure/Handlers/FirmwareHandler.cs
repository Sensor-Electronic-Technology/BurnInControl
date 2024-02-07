using BurnIn.ControlService.Infrastructure.Commands;
using BurnIn.ControlService.Infrastructure.Hubs;
using BurnIn.ControlService.Infrastructure.Services;
using BurnIn.Shared.Services;
using BurnIn.Shared.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
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

public class UpdateHandler : IRequestHandler<UpdateCommand> {
    private readonly FirmwareUpdateService _firmwareService;
    private readonly StationController _stationController;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    public UpdateHandler(FirmwareUpdateService firmwareService, StationController stationController) {
        this._firmwareService = firmwareService;
        this._stationController = stationController;
    }
    public async Task Handle(UpdateCommand request, CancellationToken cancellationToken) {
        /*var disconnectResult=await this._stationController.Disconnect();
        if (disconnectResult.IsSuccess) { 
            UpdateResponse updateResponse=await this._firmwareService.UploadFirmwareUpdate();
            var connectResult=await this._stationController.ConnectUsb();
            if (connectResult.IsSuccess) {
                await this._hubContext.Clients.All.OnFirmwareUpdated(true, response.Version, "Updated",response.UploadText);
                return new UpdateResponse(updateResponse.Version,updateResponse.UploadText);
            } else {
                
                return new UpdateResponse("","");
            }
        } else {
            return new UpdateResponse("","");
        }*/

    }
}

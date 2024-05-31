using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class UpdateCurrentTempHandler:IRequestHandler<UpdateCurrentTempCommand> {
    private readonly IStationController _stationController;

    public UpdateCurrentTempHandler(IStationController stationController) {
        this._stationController = stationController;
    }
    
    public async Task Handle(UpdateCurrentTempCommand request, CancellationToken cancellationToken) {
        await this._stationController.Send(StationMsgPrefix.UpdateCurrentPrefix, new IntValuePacket(){Value=request.Current});
        await Task.Delay(200, cancellationToken);
        await this._stationController.Send(StationMsgPrefix.UpdateTempPrefix,new IntValuePacket(){Value=request.Temperature});
    }
}
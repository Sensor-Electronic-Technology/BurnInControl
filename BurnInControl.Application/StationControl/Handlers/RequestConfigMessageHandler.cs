using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class RequestConfigMessageHandler:IRequestHandler<RequestConfigMessage> {
    private IStationController _stationController;
    
    public RequestConfigMessageHandler(IStationController stationController) {
        this._stationController = stationController;
    }
    
    public Task Handle(RequestConfigMessage request, CancellationToken cancellationToken) {
        return this._stationController.Send(StationMsgPrefix.GetConfigPrefix, request.ConfigType);
    }
}
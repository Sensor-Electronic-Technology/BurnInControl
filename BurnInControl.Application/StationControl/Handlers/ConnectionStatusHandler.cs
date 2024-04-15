using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class ConnectionStatusHandler:IRequestHandler<RequestConnectionStatus> {
    private readonly IStationController _stationController;

    public ConnectionStatusHandler(IStationController stationController) {
        this._stationController = stationController;
    }
    
    public Task Handle(RequestConnectionStatus request, CancellationToken cancellationToken) {
        return this._stationController.GetConnectionStatus();
    }
}
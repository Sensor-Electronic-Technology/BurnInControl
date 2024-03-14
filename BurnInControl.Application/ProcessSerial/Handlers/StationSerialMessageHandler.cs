using BurnInControl.Application.ProcessSerial.Interfaces;
using BurnInControl.Application.ProcessSerial.Messages;
using MediatR;
namespace BurnInControl.Application.ProcessSerial.Handlers;

public class StationSerialMessageHandler:IRequestHandler<StationMessage> {
    private IStationMessageHandler _stationMessageHandler;
    public StationSerialMessageHandler(IStationMessageHandler stationMessageHandler) {
        this._stationMessageHandler = stationMessageHandler;
    }
    public Task Handle(StationMessage stationMessage, CancellationToken cancellationToken) {
        return this._stationMessageHandler.Handle(stationMessage,cancellationToken);
    }
}
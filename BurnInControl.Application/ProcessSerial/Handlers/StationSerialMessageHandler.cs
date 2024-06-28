using BurnInControl.Application.ProcessSerial.Interfaces;
using BurnInControl.Application.ProcessSerial.Messages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BurnInControl.Application.ProcessSerial.Handlers;

public class StationSerialMessageHandler:IRequestHandler<StationMessage> {
    private IStationMessageHandler _stationMessageHandler;
    private ILogger<StationSerialMessageHandler> _logger;
    public StationSerialMessageHandler(IStationMessageHandler stationMessageHandler) {
        this._stationMessageHandler = stationMessageHandler;
    }
    public async Task Handle(StationMessage stationMessage, CancellationToken cancellationToken) {
        try {
            await this._stationMessageHandler.Handle(stationMessage,cancellationToken);
        } catch(Exception e) {
            this._logger.LogError(e,"Exception thrown while handling serial message");
        }
    }
}
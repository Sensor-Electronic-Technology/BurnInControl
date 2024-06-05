using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class SendTuningWindowSizeHandler:IRequestHandler<SendTuningWindowSizeCommand> {
    private readonly IStationController _stationController;
    public SendTuningWindowSizeHandler(IStationController stationController) {
        this._stationController = stationController;
    }
    public Task Handle(SendTuningWindowSizeCommand request, CancellationToken cancellationToken) {
        return this._stationController.Send(StationMsgPrefix.SendRunningTestPrefix,
            new IntValuePacket(){Value= request.WindowSize});
    }
}
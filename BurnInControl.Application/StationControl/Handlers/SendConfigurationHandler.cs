using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Handlers;

public class SendConfigurationHandler : IRequestHandler<SendConfiguration> {
    private readonly IStationController _stationController;

    public SendConfigurationHandler(IStationController stationController) {
        this._stationController = stationController;
    }
    
    public async Task Handle(SendConfiguration request, CancellationToken cancellationToken) {
        switch (request.Configuration) {
            case HeaterControllerConfig heaterControllerConfig: {
                await this._stationController.Send(StationMsgPrefix.HeaterConfigPrefix, heaterControllerConfig);
                break;
            }
            case ProbeControllerConfig probeControllerConfig: {
                await this._stationController.Send(StationMsgPrefix.ProbeConfigPrefix, probeControllerConfig);
                break;
            }
            case StationConfiguration stationConfiguration: {
                await this._stationController.Send(StationMsgPrefix.StationConfigPrefix, stationConfiguration);
                break;
            }
        }
    }
}
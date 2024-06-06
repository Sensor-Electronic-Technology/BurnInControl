using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.ComponentConfiguration;
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
                Console.WriteLine("Sending heater config..");
                await this._stationController.Send(StationMsgPrefix.ReceiveConfigPrefix,
                    new ConfigPacket<HeaterControllerConfig>() {
                        ConfigType = ConfigType.HeaterControlConfig,
                        Configuration = heaterControllerConfig
                    });
                break;
            }
            case ProbeControllerConfig probeControllerConfig: {
                Console.WriteLine("Sending probe config...");
                await this._stationController.Send(StationMsgPrefix.ReceiveConfigPrefix,
                    new ConfigPacket<ProbeControllerConfig>() {
                        ConfigType = ConfigType.ProbeControlConfig,
                        Configuration = probeControllerConfig
                    });
                break;
            }
            case StationConfiguration stationConfiguration: {
                Console.WriteLine("Sending station config...");
                await this._stationController.Send(StationMsgPrefix.ReceiveConfigPrefix,
                    new ConfigPacket<StationConfiguration>() {
                        ConfigType = ConfigType.ControllerConfig,
                        Configuration = stationConfiguration
                    });
                break;
            }
        }
    }
}
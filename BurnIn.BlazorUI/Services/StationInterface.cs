using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using Wolverine;
namespace BurnIn.BlazorUI.Services;

public class StationInterface {
    private readonly IMessageBus _messageBus;
    private readonly ILogger<StationInterface> _logger;
    
    public StationInterface(IMessageBus messageBus, ILogger<StationInterface> logger) {
        this._messageBus = messageBus;
        this._logger = logger;
    }

    public async Task<ErrorOr<Success>> SendStart() {
        return await this._messageBus.InvokeAsync<ErrorOr<Success>>(new SendStationCommand() {
            Command=StationCommand.Start
        });
    }
    
    public async Task<ErrorOr<Success>> SendContinue() {
        return await this._messageBus.InvokeAsync<ErrorOr<Success>>(new SendStationCommand() {
            Command=StationCommand.Start
        });
    }
    
    public async Task<ErrorOr<Success>> SendReset() {
        return await this._messageBus.InvokeAsync<ErrorOr<Success>>(new SendStationCommand() {
            Command=StationCommand.Reset
        });
    }
}
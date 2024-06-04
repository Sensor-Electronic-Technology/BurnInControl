using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace BurnInControl.Application.StationControl.Handlers;

public class SaveTuningResultsHandler:IRequestHandler<SaveTuningResultsCommand> {
    private readonly StationDataService _stationDataService;
    private readonly IStationController _stationController;
    private IHubContext<StationHubController,IStationHub> _hubContext;
    private IConfiguration _configuration;
    
    public SaveTuningResultsHandler(StationDataService stationDataService,
        IStationController stationController,
        IHubContext<StationHubController,IStationHub> hubContext,
        IConfiguration configuration) {
        this._stationDataService = stationDataService;
        this._hubContext = hubContext;
        this._stationController = stationController;
        this._configuration = configuration;
    }


    public async Task Handle(SaveTuningResultsCommand request, CancellationToken cancellationToken) {
        var stationId = this._configuration["StationId"] ?? "S01";
        var saveSuccess=await this._stationDataService.SaveTuningResults(stationId,request.Results);
        if (saveSuccess) {
            var response=await this._stationController.Send(StationMsgPrefix.CommandPrefix, StationCommand.SaveTuning);
            if (response.IsError) {
                await this._hubContext.Clients.All.OnTuningResultsSavedDatabase(false,
                    "Results saved to database but failed to send to the controller.");

            } else {
                await this._hubContext.Clients.All.OnTuningResultsSavedDatabase(true,"Results saved to the database. " +
                    "Sending to controller");
            }
        } else {
            await this._hubContext.Clients.All.OnTuningResultsSavedDatabase(false,"Failed to save tuning results to the database." +
                                                                    " Data not sent to controller");
        }
    }
}
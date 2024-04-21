using BurnInControl.Application.FirmwareUpdate.Messages;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.FirmwareData;
using Coravel.Scheduling.Schedule.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StationService.Infrastructure.Firmware.Jobs;
using StationService.Infrastructure.Hub;

namespace StationService.Infrastructure.Firmware;

public class UpdateWatcher:BackgroundService {
    private readonly IMediator _mediator;
    private IHubContext<StationHub,IStationHub> _hubContext;
    private readonly IScheduler _scheduler;
    private readonly IConfiguration _Configuration;
    private readonly IMongoCollection<StationFirmwareTracker> _trackerCollection;
    private readonly ILogger<UpdateWatcher> _logger;
    private string _stationId = string.Empty;
    
    public UpdateWatcher(IMediator mediator,
        IConfiguration configuration,
        IScheduler scheduler,
        IMongoClient client,
        ILogger<UpdateWatcher> logger,
        IOptions<DatabaseSettings> options,
        IHubContext<StationHub,IStationHub> hubContext){
        this._mediator = mediator;
        this._scheduler= scheduler;
        this._Configuration = configuration;
        this._hubContext = hubContext;
        this._logger = logger;
        this._trackerCollection = client.GetDatabase(options.Value.DatabaseName ?? "burn_in_db")
            .GetCollection<StationFirmwareTracker>(options.Value.TrackerCollectionName ?? "station_update_tracker");
        this._stationId= this._Configuration["StationId"] ?? "S01";  //TODO: Replace S01 with S00
    } 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var cursor =await this._trackerCollection.WatchAsync(options:new ChangeStreamOptions() {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        },cancellationToken: stoppingToken);
        foreach(var change in cursor.ToEnumerable()) {
            var tracker=change.FullDocument;
            if (!tracker.UpdateAvailable || tracker.StationId != this._stationId) continue;
            this._logger.LogInformation("Firmware update available for station");
            await this._mediator.Send(new TryUpdateFirmwareCommand(), stoppingToken);
        }
    }
}
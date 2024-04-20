using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.Shared.FirmwareData;
using Coravel.Scheduling.Schedule.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        IHubContext<StationHub,IStationHub> hubContext){
        this._mediator = mediator;
        this._scheduler= scheduler;
        this._Configuration = configuration;
        this._hubContext = hubContext;
        this._logger = logger;
        this._trackerCollection = client.GetDatabase("burn_in_db")
            .GetCollection<StationFirmwareTracker>("station_update_tracker");
        this._stationId= this._Configuration["StationId"] ?? "S01";
    } 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var cursor =await this._trackerCollection.WatchAsync(cancellationToken: stoppingToken);
        foreach(var change in cursor.ToEnumerable()) {
            var tracker=change.FullDocument;
            if (tracker.UpdateAvailable) {
                this._logger.LogInformation("Firmware update available for station, update job scheduled to run once " +
                                            "a test is not running"); 
                this._scheduler.Schedule<FirmwareUpdateJob>()
                    .Hourly().RunOnceAtStart();
            }
        }
    }
}
using System.Net;
using System.Timers;
using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Messages;
using BurnInControl.Application.StationControl.Interfaces;
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
using Timer = System.Timers.Timer;

namespace StationService.Infrastructure.Firmware;

public class UpdateWatcher:IHostedService {
    private readonly long _timerOffset = 60000;
    private IHubContext<StationHub,IStationHub> _hubContext;
    private readonly ILogger<UpdateWatcher> _logger;
    private FileSystemWatcher _watcher;
    private readonly ITestService _testService;
    private readonly IFirmwareUpdateService _firmwareUpdateService;
    private readonly IStationController _stationController;
    private readonly HttpClient _httpClient = new HttpClient();
    private Timer _serviceUpdateTimer;
    private Timer _firmwareUpdateTimer;

    private bool _firmwareUpdateAvailable = false;
    private bool _serviceUpdateAvailable = false;
    private readonly UpdateSettings _updateSettings;
    
    public UpdateWatcher(IOptions<UpdateSettings> settings, ILogger<UpdateWatcher> logger,
        ITestService testService, IFirmwareUpdateService firmwareUpdateService,
        IStationController stationController,
        IHubContext<StationHub,IStationHub> hubContext){
        this._hubContext = hubContext;
        this._logger = logger;
        this._firmwareUpdateService = firmwareUpdateService;
        this._stationController = stationController;
        this._updateSettings = settings.Value;
        this._watcher = new FileSystemWatcher();
        this._watcher.Path = this._updateSettings.UpdateDirectory ?? "/updates/";
        this._testService = testService;
        this._watcher.NotifyFilter = NotifyFilters.FileName;
        this._watcher.Created += this.OnCreated;
        this._firmwareUpdateTimer = new Timer();
        this._firmwareUpdateTimer.AutoReset = false;
        this._firmwareUpdateTimer.Elapsed += OnFirmwareUpdateTimer;
        
        this._serviceUpdateTimer = new Timer();
        this._serviceUpdateTimer.AutoReset = false;
        this._serviceUpdateTimer.Elapsed += OnServiceUpdateTimer;
        
        this._httpClient.BaseAddress = new Uri(this._updateSettings.UpdateApiUrl);
        this._httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._updateSettings.UpdateApiToken ?? "station-soft-token");
    }
    public Task StartAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("UpdateWatcher started");
        this._watcher.IncludeSubdirectories = true;
        this._watcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }
    
    private void OnCreated(object source,FileSystemEventArgs e) {
        this._logger.LogInformation("File: {0} {1}",e.FullPath,e.ChangeType);
        if (e.ChangeType == WatcherChangeTypes.Created) {
            this._logger.LogInformation("File created: {FileName}",e.Name);
            if (!string.IsNullOrEmpty(e.Name)) {
                if(e.Name.Contains(this._updateSettings.FirmwareUpdateFileName ?? "BurnInFirmwareV3.ino.hex")) {
                    if (this._testService.IsRunning) {
                        var deadline = this._testService.RemainingTimeSecs();
                        this._firmwareUpdateTimer.Interval= (deadline*1000) + this._timerOffset;
                        this._firmwareUpdateAvailable = true;
                        this._firmwareUpdateTimer.Start();
                    } else {
                        this.UpdateFirmware();
                    }
                }

                if (e.Name.Contains(this._updateSettings.ServiceUpdateFileName ?? "service_update.txt")) {
                    if (this._testService.IsRunning) {
                        var deadline = this._testService.RemainingTimeSecs();
                        this._serviceUpdateTimer.Interval = (deadline * 1000) + this._timerOffset;
                        this._serviceUpdateAvailable = true;
                        if (!this._firmwareUpdateAvailable) {
                            this._serviceUpdateTimer.Start();
                        }
                    } else {
                        this.UpdateService();
                    }
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("UpdateWatcher stopped");
        this._watcher.EnableRaisingEvents = false;
        this._firmwareUpdateTimer.Stop();
        this._serviceUpdateTimer.Stop();
        return Task.CompletedTask;
    }
    
    private void OnFirmwareUpdateTimer(object? source, ElapsedEventArgs e) {
        this._firmwareUpdateAvailable = false;
        this._firmwareUpdateService.UploadFirmwareStandAlone().Wait();
        if (this._serviceUpdateAvailable) {
            this._serviceUpdateAvailable = false;
            this.UpdateService();
        }
    }

    private void UpdateService() {
        using var request = new HttpRequestMessage(new HttpMethod("GET"), "http://localhost:8080/v1/update");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {this._updateSettings.UpdateApiToken}"); 
        var response = this._httpClient.Send(request);
        if (response.StatusCode == HttpStatusCode.OK) {
            this._logger.LogInformation("Service update request sent");
        } else {
            this._logger.LogError("Service update request failed");
        }
    }

    private void UpdateFirmware() {
        this._logger.LogInformation("Update available, disconnecting station and uploading firmware update");
        this._stationController.Disconnect().Wait();
        this._firmwareUpdateService.UploadFirmwareStandAlone().Wait();
        this._stationController.ConnectUsb().Wait();
    }
    
    private void OnServiceUpdateTimer(object? source, ElapsedEventArgs e) {
        if(this._firmwareUpdateAvailable) {
            this._serviceUpdateAvailable = true;
        }
    }
}

/*public class UpdateWatcher:BackgroundService {
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
}*/
using System.Diagnostics;
using System.Timers;
using AsyncAwaitBestPractices;
using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.Shared;
using BurnInControl.Shared.AppSettings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    }
    public Task StartAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("UpdateWatcher started");
        this._watcher.IncludeSubdirectories = true;
        this._watcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("UpdateWatcher stopped");
        this._watcher.EnableRaisingEvents = false;
        this._firmwareUpdateTimer.Stop();
        this._serviceUpdateTimer.Stop();
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

                if (e.Name.Contains(this._updateSettings.ServiceUpdateFileName ?? "service_update.txt")
                    || e.Name.Contains(this._updateSettings.UiUpdateFileName ?? "ui_update.txt")) {
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


    
    private void OnFirmwareUpdateTimer(object? source, ElapsedEventArgs e) {
        this._firmwareUpdateAvailable = false;
        this.UpdateFirmware();
        if (this._serviceUpdateAvailable) {
            this._serviceUpdateAvailable = false;
            this.UpdateService();
        }
    }

    private void UpdateService() {
        using Process process = new Process();
        var command = "core install arduino:avr";
        process.StartInfo.FileName = "curl";
        process.StartInfo.Arguments = "-H \"Authorization: Bearer station-soft-token\" http://10.5.0.12:8080/v1/update";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        try {
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(result);
        } catch(Exception e) {
            this._logger.LogError("Error while updating StationService Exception: \n  {ErrorMessage}", e.ToErrorMessage());
            this._hubContext.Clients.All.OnFirmwareUpdateFailed($"Exception thrown while updating StationService: /n {e.ToErrorMessage()}").SafeFireAndForget();
        }
        this.DeleteUpdateFile(this._updateSettings.ServiceUpdateFileName ?? "service_update.txt");
        this.DeleteUpdateFile(this._updateSettings.UiUpdateFileName ?? "ui_update.txt");
    }

    private void DeleteUpdateFile(string fileName) {
        try {
            File.Delete(Path.Combine(this._updateSettings.UpdateDirectory ?? "/updates/",
                fileName));
        } catch (Exception e) {
            this._logger.LogError("Failed to delete update file {FileName}",fileName);
        }
    }

    private void UpdateFirmware() {
        this._logger.LogInformation("Update available, disconnecting station and uploading firmware update");
        this._stationController.Disconnect().Wait();
        this._firmwareUpdateService.UploadFirmwareStandAlone().Wait();
        this._stationController.ConnectUsb().Wait();
        this.DeleteUpdateFile(this._updateSettings.FirmwareUpdateFileName ?? "BurnInFirmwareV3.ino.hex");
    }
    
    private void OnServiceUpdateTimer(object? source, ElapsedEventArgs e) {
        if(this._firmwareUpdateAvailable) {
            this._serviceUpdateAvailable = true;
        }
    }
}
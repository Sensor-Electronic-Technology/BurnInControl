using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.FirmwareData;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.Diagnostics;
using System.Text.RegularExpressions;
using BurnInControl.Data.VersionModel;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Shared;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using StationService.Infrastructure.Firmware.Jobs;
using StationService.Infrastructure.Hub;
using FileMode=System.IO.FileMode;

namespace StationService.Infrastructure.Firmware;

public class FirmwareUpdateService:IFirmwareUpdateService {
    private readonly Regex _regex = new Regex("^V\\d\\.\\d\\.\\d$", RegexOptions.IgnoreCase);
    private readonly ILogger<FirmwareUpdateService> _logger;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly StationDataService _stationDataService;
    private readonly GitHubClient _github;
    private UpdateCheckStatus _updateCheckStatus=new UpdateCheckStatus();
    private readonly IMongoCollection<VersionLog> _versionCollection;
    
    private readonly string _org;
    private readonly string _repo;
    private readonly string _firmwarePath;
    private readonly string _firmwareFileName;
    private readonly string _avrDudeCommand;
    private readonly string _avrDudeFileName;
    private readonly string _firmwareFullPath;
    public bool UpdateAvailable => this._updateCheckStatus.UpdateAvailable;
    
    public FirmwareUpdateService(ILogger<FirmwareUpdateService> logger,
        IMongoClient client,
        IHubContext<StationHub, IStationHub> hubContext,
        IOptions<FirmwareUpdateSettings> options,
        IOptions<DatabaseSettings> dbOptions,
        StationDataService stationDataService){
        this._logger = logger;
        this._hubContext = hubContext;
        this._stationDataService=stationDataService;
        this._org = options.Value.GithubOrg;
        this._repo = options.Value.GithubRepo;
        this._firmwarePath = options.Value.FirmwarePath;
        this._firmwareFileName = options.Value.FirmwareFileName;
        this._avrDudeCommand = options.Value.AvrDudeCmd;
        this._avrDudeFileName = options.Value.AvrDudeFileName;
        this._firmwareFullPath = this._firmwarePath + this._firmwareFileName;
        this._versionCollection = client.GetDatabase(dbOptions.Value.DatabaseName ?? "burn_in_db")
            .GetCollection<VersionLog>(dbOptions.Value.VersionCollectionName ?? "version_log");
        this._github=new GitHubClient(new ProductHeaderValue(this._org));
    }
    public async Task GetLatestVersion() {
        var latest=await this._github.Repository.Release.GetLatest(this._org, this._repo);
        var current = await this._versionCollection.Find(e => true).FirstAsync();
        /*var stationId=Environment.GetEnvironmentVariable("StationId");
        var current=await this._stationDataService.GetFirmwareVersion(stationId);*/
        if (latest != null && !string.IsNullOrEmpty(latest.TagName)) {
            if (current != null && !string.IsNullOrEmpty(current.Version)) {
                this._updateCheckStatus.SetUpdateAvailable(latest.TagName,current.Version);
                await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
            } else {
                var lateCheck = await this._versionCollection
                                .Find(e => e.Version == latest.TagName)
                                .FirstOrDefaultAsync();
                if(lateCheck!=null) {
                    await this._versionCollection.DeleteOneAsync(e=>e._id==lateCheck._id);
                }
                this._updateCheckStatus.SetUpdateAvailableWithMessage("Warning: Current version not found in database",latest.TagName);
                await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
            };
        } else {
            if (current != null) {
                this._updateCheckStatus.SetError("Error: Latest version not found",current.Version);
            } else {
                this._updateCheckStatus.SetError("Error: Latest version not found");
            }
        }
        await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
    }

    public async Task<UpdateCheckStatus> CheckForUpdate() {
        var latest=await this._github.Repository.Release.GetLatest(this._org, this._repo);
        var current = await this._versionCollection.Find(e => true).FirstAsync();
        if (latest != null && !string.IsNullOrEmpty(latest.TagName)) {
            if (current != null && !string.IsNullOrEmpty(current.Version)) {
                this._updateCheckStatus.SetUpdateAvailable(latest.TagName,current.Version);
                await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
            } else {
                var lateCheck = await this._versionCollection
                    .Find(e => e.Version == latest.TagName)
                    .FirstOrDefaultAsync();
                if(lateCheck!=null) {
                    await this._versionCollection.DeleteOneAsync(e=>e._id==lateCheck._id);
                }
                this._updateCheckStatus.SetUpdateAvailableWithMessage("Warning: Current version not found in database",latest.TagName);
                await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
            };
        } else {
            if (current != null) {
                this._updateCheckStatus.SetError("Error: Latest version not found",current.Version);
            } else {
                this._updateCheckStatus.SetError("Error: Latest version not found");
            }
            
        }
        await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
        return this._updateCheckStatus;
    }

    public async Task UploadFirmwareUpdate() {
        if (await this.DownloadFirmwareUpdate()) {
            using Process process = new Process();
            process.StartInfo.FileName = this._avrDudeFileName;
            process.StartInfo.Arguments = this._avrDudeCommand;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            try {
                process.Start();
                var result = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                Console.WriteLine(result);
                this._updateCheckStatus.SetUpdated();
                var updateStatus = new UpdateStatus();
                updateStatus.SetUpdateStatus(this._updateCheckStatus.CurrentVersion ?? "Unknown",result);
            } catch(Exception e) {
                string exMessage = e.Message;
                if (e.InnerException != null) {
                    exMessage+=e.InnerException.Message;
                }
                this._logger.LogError(exMessage);
                this._updateCheckStatus.SetError($"Exception thrown while updating firmware: /n {exMessage}");
            }
        }
    }
    
    private async Task<bool> DownloadFirmwareUpdate() {
        if (!this.UpdateAvailable) {
            await this._hubContext.Clients.All.OnFirmwareDownloaded(false, "No update available");
            return false;
        }
        var release = await this._github.Repository.Release.Get(this._org, this._repo,this._updateCheckStatus.AvailableVersion);
        HttpClient client = new HttpClient();
        if (release.Assets.Any()) {
            var asset = release.Assets.FirstOrDefault(e => e.Name == this._firmwareFileName);
            if (asset != null) {
                var uri = new Uri(asset.BrowserDownloadUrl);
                try {
                    var stream = await client.GetStreamAsync(uri);
                    if (File.Exists(this._firmwareFullPath)) {
                        File.Delete(this._firmwareFullPath);
                        this._logger.LogInformation("Old firmware file deleted");
                    }
                    await using var fs = new FileStream(this._firmwareFullPath, FileMode.Create);
                    await stream.CopyToAsync(fs);
                    this._logger.LogInformation("Firmware downloaded");
                    await this._hubContext.Clients.All.OnFirmwareDownloaded(true, "Firmware downloaded");
                    return true;
                } catch (Exception exception) {
                    string message = $"Failed to download firmware: {exception.ToErrorMessage()}";
                    this._logger.LogError(message);
                    await this._hubContext.Clients.All.OnFirmwareDownloaded(false, message);
                    return false;
                }
            } else {
                this._logger.LogError("Failed to download, file BurnInFirmwareV3.ino.hex not found");
                await this._hubContext.Clients.All.OnFirmwareDownloaded(false, "Failed to download, " +
                                                                               "release asset collection was empty");
                return false;
            }
        } else {
            this._logger.LogError("Failed to download latest firmware");
            await this._hubContext.Clients.All.OnFirmwareDownloaded(false,"Failed to download latest firmware");
            return false;
        }
    }
}
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.FirmwareData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.Infrastructure.FirmwareModel;
using BurnInControl.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using StationService.Infrastructure.Hub;
using FileMode=System.IO.FileMode;

namespace StationService.Infrastructure.Firmware;

public class FirmwareUpdateService:IFirmwareUpdateService {
    private readonly Regex _regex = new Regex("^V\\d\\.\\d\\.\\d$", RegexOptions.IgnoreCase);
    private readonly ILogger<FirmwareUpdateService> _logger;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly FirmwareDataService _firmwareDataService;
    private readonly GitHubClient _github;
    private UpdateCheckStatus _updateCheckStatus=new UpdateCheckStatus();
    private readonly IConfiguration _configuration;
    private readonly FirmwareUpdateSettings _settings;
    
    private string _firmwareFullPath = "";
    private readonly string _stationId = "";
    public bool UpdateAvailable => this._updateCheckStatus?.UpdateAvailable ?? false;
    
    public FirmwareUpdateService(ILogger<FirmwareUpdateService> logger,
        FirmwareDataService firmwareDataService,
        IHubContext<StationHub, IStationHub> hubContext,
        IOptions<FirmwareUpdateSettings> options,
        IConfiguration configuration){
        this._logger = logger;
        this._hubContext = hubContext;
        this._firmwareDataService = firmwareDataService;
        this._configuration = configuration;
        this._settings= options.Value;
        this._github=new GitHubClient(new ProductHeaderValue(this._settings.GithubOrg));
        
        this._stationId = this._configuration["StationId"] ?? "S01"; //TODO: Change to S00 when not debugging
        this._firmwareFullPath = this._settings.FirmwarePath + this._settings.FirmwareFileName;
        
    }
    public async Task<UpdateCheckStatus> CheckForUpdate() {
        var updateCheckStatus = await this._firmwareDataService.CheckAvailable(this._stationId);
        if(updateCheckStatus==null) {
            this._updateCheckStatus = new UpdateCheckStatus();
            this._updateCheckStatus.SetError("Failed to find UpdateCheckStatus in database");
            this._logger.LogError("Failed to find UpdateCheckStatus in database");
            return this._updateCheckStatus;
        }
        this._updateCheckStatus= updateCheckStatus;
        if (this._updateCheckStatus.UpdateAvailable) {
            var release = await this._github.Repository.Release.Get(this._settings.GithubOrg, this._settings.GithubRepo,
                this._updateCheckStatus.AvailableVersion);
            if (release != null && !string.IsNullOrEmpty(release.TagName)) {
                if (release.TagName==this._updateCheckStatus.AvailableVersion) {
                    this._logger.LogInformation("Update available!");
                    await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
                } 
            } else {
                this._updateCheckStatus.SetError("Error checking for update.  Release version does not match tracker version");
                this._logger.LogError("Error checking for update.  Release version does not match tracker version");
                await this._hubContext.Clients.All.OnFirmwareUpdateCheck(this._updateCheckStatus);
            }
        }
        return this._updateCheckStatus;
    }

    public async Task UploadFirmwareUpdate() {
        if (await this.DownloadFirmwareUpdate()) {
            using Process process = new Process();
            process.StartInfo.FileName = this._settings.AvrDudeFileName;
            process.StartInfo.Arguments = this._settings.AvrDudeCmd;
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
                await this._firmwareDataService.MarkUpdated(this._stationId);
                await this._hubContext.Clients.All.OnFirmwareUpdateCompleted(this._updateCheckStatus.CurrentVersion ?? "Unknown");
            } catch(Exception e) {
                this._logger.LogError("Error while updating firmware.  Exception: \n  {ErrorMessage}", e.ToErrorMessage());
                this._updateCheckStatus.SetError($"Exception thrown while updating firmware: /n {e.ToErrorMessage()}");
                await this._hubContext.Clients.All.OnFirmwareUpdateFailed($"Exception thrown while updating firmware: /n {e.ToErrorMessage()}");
            }
        }
    }
    
    private async Task<bool> DownloadFirmwareUpdate() {
        if (!this.UpdateAvailable) {
            await this._hubContext.Clients.All.OnFirmwareDownloaded(false, "No update available");
            return false;
        }
        var release = await this._github.Repository.Release.Get(this._settings.GithubOrg, this._settings.GithubRepo,
                                    this._updateCheckStatus.AvailableVersion);
        HttpClient client = new HttpClient(); //Download file client
        if (release.Assets.Any()) {
            var asset = release.Assets.FirstOrDefault(e => e.Name == this._settings.FirmwareFileName);
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
                } catch (Exception exception) { ;
                    this._logger.LogError("Failed to download firmware: {ErrorMessage}", exception.ToErrorMessage());
                    await this._hubContext.Clients.All.OnFirmwareDownloaded(false, $"Failed to download firmware: {exception.ToErrorMessage()}");
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
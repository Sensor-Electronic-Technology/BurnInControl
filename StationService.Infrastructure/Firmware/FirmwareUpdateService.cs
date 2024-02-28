using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.FirmwareData;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FileMode=System.IO.FileMode;

namespace StationService.Infrastructure.Firmware;

public class FirmwareUpdateService:IFirmwareUpdateService {
    private readonly Regex _regex = new Regex("^V\\d\\.\\d\\.\\d$", RegexOptions.IgnoreCase);
    private readonly ILogger<FirmwareUpdateService> _logger;
    private readonly GitHubClient _github;
    private string _latestVersion = string.Empty;
    private FirmwareUpdateStatus _firmwareUpdateStatus=new FirmwareUpdateStatus();
    
    private readonly string _org;
    private readonly string _repo;
    private readonly string _firmwarePath;
    private readonly string _firmwareFileName;
    private readonly string _avrDudeCommand;
    private readonly string _avrDudeFileName;
    private readonly string _firmwareFullPath;

    public string Version => (!string.IsNullOrEmpty(this._latestVersion) ? this._latestVersion : "V0.0.0");

    public FirmwareUpdateService(ILogger<FirmwareUpdateService> logger,
        IOptions<FirmwareUpdateSettings> options) {
        this._logger = logger;
        this._org = options.Value.GithubOrg;
        this._repo = options.Value.GithubRepo;
        this._firmwarePath = options.Value.FirmwarePath;
        this._firmwareFileName = options.Value.FirmwareFileName;
        this._avrDudeCommand = options.Value.AvrDudeCmd;
        this._avrDudeFileName = options.Value.AvrDudeFileName;
        this._firmwareFullPath = this._firmwarePath + this._firmwareFileName;
        this._github=new GitHubClient(new ProductHeaderValue(this._org));
    }
    
    public FirmwareUpdateService() {
        this._org = "Sensor-Electronic-Technology";
        this._repo = "BurnInFirmware";
        this._firmwarePath = "/source/ControlUpload/";
        this._firmwareFileName = "BurnInFirmwareV3.ino.hex";
        this._avrDudeCommand = "-C /source/ControlUpload/avrdude.conf -v -p m2560 -c stk500v2 -P /dev/ttyACM0 -b 115200 -D -U flash:w:/source/ControlUpload/BurnInFirmwareV3.ino.hex";
        this._avrDudeFileName = "avrdude";
        this._firmwareFullPath = this._firmwarePath + this._firmwareFileName;
        this._github=new GitHubClient(new ProductHeaderValue(this._org));
    }
    
    public async Task CreateNewRelease(string versionTag,string name,string description) {
        //Create Release
        var newRelease = new NewRelease(versionTag) {
            Name = name,
            Body = description,
            Draft = false,
            Prerelease = false,
        };
        await using var archiveContents = File.OpenRead("C:\\Users\\aelmendo\\Documents\\Arduino\\burn-build\\BurnInFirmwareV3.ino.hex");
        
        var assetUpload = new ReleaseAssetUpload() 
        {
            FileName = "BurnInFirmwareV3.ino.hex",
            ContentType = "application/file",
            RawData = archiveContents
        };
        var result = await this._github.Repository.Release.Create(this._org, this._repo, newRelease);
        await this._github.Repository.Release.UploadAsset(result, assetUpload);
    }
    
    public async Task<ErrorOr<string>> GetLatestVersion() {
        try {
            var release=await this._github.Repository.Release.GetLatest(this._org, this._repo);
            if(release!=null) {
                if (string.IsNullOrEmpty(release.TagName)) {
                    return Error.Unexpected(description:"Release TagName was null or empty");
                }
                if(!this._regex.IsMatch(release.TagName)) {
                    return Error.Validation(description: $"Version was not in correct format:  Found: {release.TagName} Correct: V#.##");
                }
                this._latestVersion = release.TagName;
                return this._latestVersion;
            } else {
                return Error.NotFound(description:"No release found");
            }
        } catch(Exception e) {
            var exMessage = e.Message;
            if(e.InnerException!=null) {
                exMessage+=e.InnerException.Message;
            }
            return Error.Failure($"Failed to get latest version. Github Api threw an exception. \n Exception: {exMessage}");
        }
    }
    
    public FirmwareUpdateStatus CheckNewerVersion(string fromController) {
        string latest = this._latestVersion;
        var controlMatch=this._regex.IsMatch(fromController);
        var latestMatch = this._regex.IsMatch(latest);
        if (!controlMatch || !latestMatch) {
            string msg = (!controlMatch) ? 
                $"Controller version doesn't fit version pattern, Correct: V#.## Latest: {fromController}" 
                : $"Github version doesn't fit version pattern, Correct: V#.## Latest: {latest}";
            this._firmwareUpdateStatus.UpdateReady = false;
            this._firmwareUpdateStatus.Message = msg;
            this._firmwareUpdateStatus.Type = UpdateType.None;
            return this._firmwareUpdateStatus;
        }
        if (latest == fromController) {
            this._firmwareUpdateStatus.SetNone("Firmware is up to data");
            return this._firmwareUpdateStatus;
        }
        string control = fromController.ToUpper();
        latest = latest.ToUpper();
        var latestSpan = latest.AsSpan();
        var controlSpan = control.AsSpan();
        
        int latestV = Convert.ToInt16(latestSpan[1]);
        int controlV = Convert.ToInt16(controlSpan[1]);
        if (latestV > controlV) {
            this._firmwareUpdateStatus.Set(UpdateType.Major,
                $"Firmware major update is available. Controller: {fromController} Latest: {latest}");
            return this._firmwareUpdateStatus;
        }
        
        latestV = Convert.ToInt16(latestSpan[3]);
        controlV = Convert.ToInt16(controlSpan[3]);
        if (latestV > controlV) {
            this._firmwareUpdateStatus.Set(UpdateType.Minor,
                $"Firmware minor update is available. Controller: {fromController} Latest: {latest}");
            return this._firmwareUpdateStatus;
        }
        
        latestV = Convert.ToInt16(latestSpan[5]);
        controlV = Convert.ToInt16(controlSpan[5]);
        if (latestV > controlV) {
            this._firmwareUpdateStatus.Set(UpdateType.Patch,
                $"Firmware patch update is available. Controller: {fromController} Latest: {latest}");
            return this._firmwareUpdateStatus;
        }
        
        this._firmwareUpdateStatus.SetNone("No Updates Available");
        return this._firmwareUpdateStatus;
    }
    
    public async Task<ErrorOr<(string ver,string avrOutput)>> UploadFirmwareUpdate() {
        using Process process = new Process();
        process.StartInfo.FileName = this._avrDudeFileName;
        process.StartInfo.Arguments = this._avrDudeCommand;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        try {
            process.Start();
            var result = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return (this.Version,result);
        }catch(Exception e) {
            string exMessage = e.Message;
            if (e.InnerException != null) {
                exMessage+=e.InnerException.Message;
            }
            return Error.Failure(description: $"Exception thrown while uploading firmware.\n Exception: {exMessage}");
        }
    }
    
    public async Task<ErrorOr<Success>> DownloadFirmwareUpdate() {
        var release = await this._github.Repository.Release.Get(this._org, this._repo,this._latestVersion);
        HttpClient client = new HttpClient();
        if (release.Assets.Any()) {
            var asset = release.Assets.FirstOrDefault(e => e.Name == this._firmwareFileName);
            if (asset != null) {
                var uri = new Uri(asset.BrowserDownloadUrl);
                var stream = await client.GetStreamAsync(uri);
                if (File.Exists(this._firmwareFullPath)) {
                    File.Delete(this._firmwareFullPath);
                    this._logger.LogInformation("Old firmware file deleted");
                }
                await using var fs = new FileStream(this._firmwareFullPath, FileMode.Create);
                await stream.CopyToAsync(fs);
                return Result.Success;
            } else {
                this._logger.LogError("Failed to download, file {file} not found","BurnInFirmwareV3.ino.hex");
                return Error.Failure(description: $"Failed to download, file {this._firmwarePath} not found");
            }
        } else {
            this._logger.LogError("Failed to download latest firmware");
            return Error.Failure(description: "Failed to download latest firmware");
        }
    }
}
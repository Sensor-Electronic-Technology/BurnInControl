using BurnIn.Shared.AppSettings;
using BurnIn.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FileMode=System.IO.FileMode;
namespace BurnIn.Shared.Services;

public enum UpdateType {
    Major,
    Minor,
    Patch,
    None
    
}

public class UpdateStatus {
    public bool UpdateReady { get; set; }
    public UpdateType Type { get; set; }
    public string Message { get; set; }
    public UpdateStatus() {
        this.UpdateReady = false;
        this.Type = UpdateType.None;
        this.Message = "";
    }
    public UpdateStatus(bool ready, UpdateType type, string message) {
        this.UpdateReady = ready;
        this.Type = type;
        this.Message = message;
    }

    public void SetNone(string? msg=null) {
        this.UpdateReady = false;
        this.Type = UpdateType.None;
        this.Message = !string.IsNullOrEmpty(msg) ? msg : "";
    }

    public void Set(UpdateType type, string msg) {
        this.UpdateReady = true;
        this.Type = type;
        this.Message = msg;
    }
}

public class FirmwareVersionService {
    private readonly Regex _regex = new Regex("^V[0-9]+\\.\\d\\d$", RegexOptions.IgnoreCase);
    private readonly ILogger<FirmwareVersionService> _logger;
    private readonly GitHubClient _github;
    private string _latestVersion = string.Empty;
    private UpdateStatus _updateStatus=new UpdateStatus();
    
    private readonly string _org;
    private readonly string _repo;
    private readonly string _firmwarePath;
    private readonly string _firmwareFileName;
    private readonly string _avrDudeCommand;
    private readonly string _avrDudeFileName;
    private readonly string _firmwareFullPath;

    public FirmwareVersionService(ILogger<FirmwareVersionService> logger,IOptions<FirmwareVersionSettings> options) {
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

    public async Task CreateNewDraftRelease(string versionTag,string name,string description) {
        
        //Create Draft Release
        var newRelease = new NewRelease(versionTag) {
            Name = name,
            Body = description,
            Draft = true,
            Prerelease = false
        };
        var result = await this._github.Repository.Release.Create(this._org, this._repo, newRelease);
        
        /*
         //Update release example
         var release = client.Repository.Release.Get("octokit", "octokit.net", 1);
        var updateRelease = release.ToUpdate();
        updateRelease.Draft = false;
        updateRelease.Name = "Version 1.0";
        updateRelease.TargetCommitish = "0edef870ecd885cc6506f1e3f08341e8b87370f2" // can also be a ref
        var result = await client.Repository.Release.Edit("octokit", "octokit.net", 1, updateRelease);
        
        Update Asset Example
        using(var archiveContents = File.OpenRead("output.zip")) { 
            var assetUpload = new ReleaseAssetUpload() 
            {
                 FileName = "my-cool-project-1.0.zip",
                 ContentType = "application/zip",
                 RawData = archiveContents
            };
            var release = client.Repository.Release.Get("octokit", "octokit.net", 1);
            var asset = await client.Repository.Release.UploadAsset(release, assetUpload);
        }
        */
    }

    public async Task GetLatestVersion() {
        var result=await this._github.Repository.Release.GetLatest(this._org, this._repo);
        this._latestVersion = result.TagName;
    }

    private async Task GetFirmwareUpdate() {
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
            } else {
                this._logger.LogError("Failed to download, file {file} not found","BurnInFirmwareV3.ino.hex");
            }
        } else {
            this._logger.LogError("Failed to download latest firmware");
        }
    }
    
    public UpdateStatus CheckNewerVersion(string fromController) {
        string latest = "V1.02";
        var controlMatch=this._regex.IsMatch(fromController);
        var latestMatch = this._regex.IsMatch(latest);
        if (!controlMatch || !latestMatch) {
            string msg = (!controlMatch) ? 
                $"Controller version doesn't fit version pattern, Correct: V#.## Latest: {fromController}" 
                : $"Latest doesn't fit version pattern, Correct: V#.## Latest: {latest}";
            this._updateStatus.UpdateReady = false;
            this._updateStatus.Message = msg;
            this._updateStatus.Type = UpdateType.None;
            return this._updateStatus;
        }
        if (latest == fromController) {
            this._updateStatus.SetNone("Firmware is up to data");
            return this._updateStatus;
        }
        string control = fromController.ToUpper();
        latest = latest.ToUpper();
        var latestSpan = latest.AsSpan();
        var controlSpan = control.AsSpan();
        
        int latestV = Convert.ToInt16(latestSpan[1]);
        int controlV = Convert.ToInt16(controlSpan[1]);
        if (latestV > controlV) {
            this._updateStatus.Set(UpdateType.Minor,"Firmware major update is available");
            return this._updateStatus;
        }
        
        latestV = Convert.ToInt16(latestSpan[3]);
        controlV = Convert.ToInt16(controlSpan[3]);
        if (latestV > controlV) {
            this._updateStatus.Set(UpdateType.Minor,"Firmware minor update is available");
            return this._updateStatus;
        }
        
        latestV = Convert.ToInt16(latestSpan[4]);
        controlV = Convert.ToInt16(controlSpan[5]);
        if (latestV > controlV) {
            this._updateStatus.Set(UpdateType.Patch,"Firmware patch is available");
            return this._updateStatus;
        }
        
        this._updateStatus.SetNone("No Updates Available");
        return this._updateStatus;
    }
    
    private void UploadFirmware() {
        /*avrdude -C avrdude.conf -v -p m2560 -c stk500v2 -P /dev/ttyACM0 -b 115200 -D -U flash:w:BurnInFirmwareV3.ino.hex*/
        using Process process = new Process();
        process.StartInfo.FileName = this._avrDudeFileName;
        process.StartInfo.Arguments = this._avrDudeCommand;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        Console.WriteLine(process.StandardOutput.ReadToEnd());
        process.WaitForExit();
    }

}
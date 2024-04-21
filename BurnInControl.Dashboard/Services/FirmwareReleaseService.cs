using System.Runtime.InteropServices;
using BurnInControl.Dashboard.Data;
using BurnInControl.Data.StationModel;
using BurnInControl.Infrastructure.FirmwareModel;
using BurnInControl.Shared.FirmwareData;
using Microsoft.Extensions.Options;
using Octokit;
using FileInfo = Radzen.FileInfo;

namespace BurnInControl.Dashboard.Services;
using MongoDB.Driver;
using MongoDB.Bson;

public class ReleaseResult {
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class FirmwareReleaseService {
    private readonly IMongoCollection<StationFirmwareTracker> _stationTrackerCollection;
    private readonly FirmwareDataService _firmwareDataService;
    private GitHubClient _client;
    private string _org;
    private string _repo;
    private string _token;
    private string _user;
    private string _email;
    private string _reference;
    
    public FirmwareReleaseService(FirmwareDataService firmwareDataService,
        IOptions<GitHubApiOptions> options) {
        this._firmwareDataService=firmwareDataService;
        this._org=options.Value.Org;
        this._repo=options.Value.Repo;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            this._token=Environment.GetEnvironmentVariable("GithubApiToken");
        } else {
            this._token=options.Value.Token;
        }
        this._user=options.Value.User;
        this._email=options.Value.Email;
        this._reference = options.Value.Ref;
        Console.WriteLine($"Org: {this._org} " +
                          $"Repo: {this._repo} " +
                          $"User: {this._user} " +
                          $"Token: {this._token} " +
                          $"Email: {this._email} " +
                          $"Ref: {this._reference}");
        this._client=new GitHubClient(new ProductHeaderValue(this._org));
        this._client.Credentials=new Credentials(this._user,this._token);
    }
    
    public async Task<ReleaseResult> CreateNewRelease(string version,string name,string message,string path) {
        long maxFileSize = 10 * 1024 * 1024;
        var sha = await this.GetObject();
        if(string.IsNullOrEmpty(sha)) {
            return new ReleaseResult {
                Success = false,
                Message = "No object found"
            };
        }
        var tag = new NewTag {
            Message = message,
            Tag = version,
            Type = TaggedType.Commit, // TODO: what are the defaults when nothing specified?
            Object = sha,
            Tagger = new Committer(this._user,this._email,DateTimeOffset.Now)
        };
        var tagResult=await this._client.Git.Tag.Create(this._org,this._repo,tag);
        if(tagResult==null) {
            return new ReleaseResult {
                Success = false,
                Message = "Failed to create tag"
            };
        }
        var newRelease = new NewRelease(tag.Tag);
        newRelease.MakeLatest = MakeLatestQualifier.True;
        newRelease.Name = "Version: "+tag.Tag;
        newRelease.Body = "Release: "+tag.Tag;
        newRelease.Draft = false;
        newRelease.Prerelease = false;
        var result = await this._client.Repository.Release.Create(this._org, this._repo, newRelease);
        if(result==null) {
            return new ReleaseResult {
                Success = false,
                Message = "Failed to create release"
            };
        }

        await using var stream = File.OpenRead(path);
        var assetUpload = new ReleaseAssetUpload() 
        {
            FileName = "BurnInFirmwareV3.ino.hex",
            ContentType = "application/file",
            RawData = stream
        };
    
        var release = await this._client.Repository.Release.Get(this._org,this._repo,result.Id);
        if(release==null) {
            return new ReleaseResult {
                Success = false,
                Message = "Release not found"
            };
        }
        var asset = await this._client.Repository.Release.UploadAsset(release, assetUpload);
        if(asset==null) {
            return new ReleaseResult {
                Success = false,
                Message = "Failed to upload asset"
            };
        }

        await this._firmwareDataService.CreateLatest(version);
        await this._firmwareDataService.UpdateStationTracker(version);
        return new ReleaseResult() { Success = true, Message = "Release created" };
    }

    private async Task<string> GetObject() {
        var refs = await this._client.Git.Reference.Get(this._org,this._repo,this._reference);
        return refs.Object.Sha ?? string.Empty;
    }
    
}
using BurnInControl.Dashboard.Data;
using BurnInControl.Data.VersionModel;
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
    private readonly IMongoCollection<VersionLog> _versionCollection;
    private GitHubClient _client;
    private string _org;
    private string _repo;
    private string _token;
    private string _user;
    private string _email;
    private string _reference;
    
    public FirmwareReleaseService(IMongoClient client,IOptions<GitHubApiOptions> options) {
        var database = client.GetDatabase("burn_in_db");
        this._versionCollection = database.GetCollection<VersionLog>("version_log");
        this._org=options.Value.Org;
        this._repo=options.Value.Repo;
        this._token=options.Value.Token;
        this._user=options.Value.User;
        this._email=options.Value.Email;
        this._reference = options.Value.Ref;
        Console.WriteLine($"Org: {this._org} " +
                          $"Repo: {this._repo} " +
                          $"Token: {this._token} " +
                          $"User: {this._user} " +
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

        await this.ClearLatest();
        await this.CreateLatest(version);
        return new ReleaseResult() { Success = true, Message = "Release created" };
    }

    public  Task CreateLatest(string version) {
        var split=version.Split('.');
        var startidx=version.IndexOf('V');
        var endidx=version.IndexOf('.');
        var majorStr = version.Substring(startidx + 1, (endidx - startidx) - 1);
        var minorstr = split[1];
        var patchstr = split[2];

        VersionLog versionLogEntry = new VersionLog();
        versionLogEntry._id= ObjectId.GenerateNewId();
        versionLogEntry.Version = version;
        versionLogEntry.Major = int.Parse(majorStr);
        versionLogEntry.Minor = int.Parse(minorstr);
        versionLogEntry.Patch = int.Parse(patchstr);
        versionLogEntry.Latest = true;
        return this._versionCollection.InsertOneAsync(versionLogEntry);
    }

    private async Task ClearLatest() {
        var update = Builders<VersionLog>.Update.Set(e=>e.Latest,false);
        var filter= Builders<VersionLog>.Filter.Eq(e=>e.Latest,true);
        await this._versionCollection.UpdateManyAsync(filter,update);
    }

    private async Task<string> GetObject() {
        var refs = await this._client.Git.Reference.Get(this._org,this._repo,this._reference);
        return refs.Object.Sha ?? string.Empty;
    }
    
}
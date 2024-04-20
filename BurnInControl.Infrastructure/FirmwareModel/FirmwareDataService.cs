using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.FirmwareData;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BurnInControl.Infrastructure.FirmwareModel;



public class FirmwareDataService {
    private readonly IMongoCollection<VersionLog> _versionCollection;
    private readonly IMongoCollection<StationFirmwareTracker> _trackerCollection;

    public FirmwareDataService(IMongoClient client, IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.DatabaseName);
        this._versionCollection =  database.GetCollection<VersionLog>(options.Value.VersionCollectionName);
        this._trackerCollection =  database.GetCollection<StationFirmwareTracker>(options.Value.TrackerCollectionName);
    }
    public async Task<UpdateCheckStatus?> CheckAvailable(string stationId) {
        return await this._trackerCollection.Find(e=>e.StationId==stationId)
            .Project(e=>new UpdateCheckStatus() {
                AvailableVersion = e.AvailableVersion,
                CurrentVersion = e.CurrentVersion,
                UpdateAvailable = e.UpdateAvailable
            }).FirstOrDefaultAsync();
    }
    
    public async Task UpdateStationTracker(string latest) {
        var stationTrackers=await this._trackerCollection.Find(_=>true).ToListAsync();
        foreach(var stationTracker in stationTrackers) {
            stationTracker.AvailableVersion=latest;
            stationTracker.UpdateAvailable=true;
            var update = Builders<StationFirmwareTracker>.Update
                .Set(e => e.UpdateAvailable, true)
                .Set(e => e.AvailableVersion, latest);
            var filter=Builders<StationFirmwareTracker>.Filter.Eq(e=>e.StationId,stationTracker.StationId);
            await this._trackerCollection.UpdateOneAsync(filter,update);
        }
    }

    public async Task MarkUpdated(string stationId) {
        var latest = await this._trackerCollection.Find(e => e.StationId == stationId)
            .Project(e => e.AvailableVersion)
            .FirstOrDefaultAsync();
        var update = Builders<StationFirmwareTracker>.Update.Set(e => e.UpdateAvailable, false)
            .Set(e => e.CurrentVersion,latest);
        var filter=Builders<StationFirmwareTracker>.Filter.Eq(e=>e.StationId,stationId);
        await this._trackerCollection.UpdateOneAsync(filter,update);
    }
    
    private async Task ClearLatest() {
        var update = Builders<VersionLog>.Update.Set(e=>e.Latest,false);
        var filter= Builders<VersionLog>.Filter.Eq(e=>e.Latest,true);
        await this._versionCollection.UpdateManyAsync(filter,update);
    }
    
    public async Task CreateLatest(string version) {
        await this.ClearLatest();
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
        await this._versionCollection.InsertOneAsync(versionLogEntry);
    }
    
    
}
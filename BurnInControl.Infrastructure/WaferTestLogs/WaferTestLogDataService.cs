using BurnInControl.Data.BurnInTests;
using BurnInControl.Shared.AppSettings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BurnInControl.Infrastructure.WaferTestLogs;

public class WaferTestLogDataService {
    private readonly IMongoCollection<WaferTestLog> _waferTestLogCollection;
    
    public WaferTestLogDataService(IMongoClient mongoClient,IOptions<DatabaseSettings> options) {
        var database= mongoClient.GetDatabase(options.Value.DatabaseName ?? "burn_in_db");
        this._waferTestLogCollection = database.GetCollection<WaferTestLog>(options.Value.WaferTestLogCollectionName ?? "wafer_test_logs");
    }

    public Task<bool> Exists(string waferId) {
        return this._waferTestLogCollection.Find(e => e.WaferId == waferId).AnyAsync();
    }
    
    public async Task<WaferTestLog?> GetWaferTestLog(string waferId) {
        return await this._waferTestLogCollection.Find(e => e.WaferId == waferId).FirstOrDefaultAsync();
    }

    public Task InsertWaferTest(string waferId,WaferTest waferTest) {
        var update=Builders<WaferTestLog>.Update.Push(e => e.WaferTests, waferTest);
        var filter=Builders<WaferTestLog>.Filter.Eq(e=>e.WaferId,waferId);
        return this._waferTestLogCollection.UpdateOneAsync(filter, update);
    }
    
    public Task<bool> CheckTested(string waferId,string waferPad) {
        return this._waferTestLogCollection.Find(e=>e.WaferId==waferId && e.WaferTests.Any(e=>e.Probe1Pad==waferPad || e.Probe2Pad==waferPad)).AnyAsync();
    }

    public async Task<List<string>> GetTestedPads(string waferId) {
        var p1Pads = await this._waferTestLogCollection.Find(e => e.WaferId == waferId)
            .Project(e => e.WaferTests.Select(e => e.Probe1Pad!).ToList())
            .FirstOrDefaultAsync();
        var p2Pads = await this._waferTestLogCollection.Find(e => e.WaferId == waferId)
            .Project(e => e.WaferTests.Select(e => e.Probe2Pad!).ToList())
            .FirstOrDefaultAsync();
        if(p1Pads is null || p2Pads is null) {
            if(p1Pads is null) {
                return p2Pads ?? [];
            }
            return p1Pads;
        }
        return p1Pads.Concat(p2Pads).ToList();
    }
    
    public Task InsertWaferTestLog(WaferTestLog waferTestLog) {
        return this._waferTestLogCollection.InsertOneAsync(waferTestLog);
    }

}
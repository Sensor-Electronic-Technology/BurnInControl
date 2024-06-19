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
    
    public Task InsertWaferTestLog(WaferTestLog waferTestLog) {
        return this._waferTestLogCollection.InsertOneAsync(waferTestLog);
    }

}
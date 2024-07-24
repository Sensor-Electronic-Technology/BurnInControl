using System.Globalization;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Shared.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QuickTest.Data.Models.Wafers.Enums;

namespace BurnInControl.Infrastructure.WaferTestLogs;

public class WaferTestLogDataService {
    private readonly IMongoCollection<WaferTestLog> _waferTestLogCollection;
    private readonly ILogger<WaferTestLogDataService> _logger;
    private static List<string> _pads = [PadLocation.PadLocationA.Value,PadLocation.PadLocationB.Value,
                                        PadLocation.PadLocationC.Value, PadLocation.PadLocationR.Value,
                                        PadLocation.PadLocationL.Value,PadLocation.PadLocationT.Value,
                                        PadLocation.PadLocationG.Value];
    
    public WaferTestLogDataService(IMongoClient mongoClient,
        ILogger<WaferTestLogDataService> logger,
        IOptions<DatabaseSettings> options) {
        var database= mongoClient.GetDatabase(options.Value.DatabaseName ?? "burn_in_db");
        this._waferTestLogCollection = database.GetCollection<WaferTestLog>(options.Value.WaferTestLogCollectionName ?? 
                                                                            "wafer_test_logs");
        this._logger = logger;
    }
    
    public WaferTestLogDataService(IMongoClient mongoClient) {
        var database= mongoClient.GetDatabase("burn_in_db");
        this._waferTestLogCollection = database.GetCollection<WaferTestLog>("wafer_test_logs");
    }

    public Task<bool> Exists(string waferId) {
        return this._waferTestLogCollection.Find(e => e.WaferId == waferId).AnyAsync();
    }
    
    public async Task<WaferTestLog?> GetWaferTestLog(string waferId) {
        return await this._waferTestLogCollection.Find(e => e.WaferId == waferId).FirstOrDefaultAsync();
    }
    
    public async Task Insert(string waferId,WaferTest waferTest) {
        var waferTestLog =await this._waferTestLogCollection.Find(e => e.WaferId == waferId).FirstOrDefaultAsync();
        if (waferTestLog is null) {
            WaferTestLog newLog = new WaferTestLog() {
                WaferId = waferId, 
                WaferPadInitialData = waferTest.WaferPadInitialData,
                WaferPadFinalData = waferTest.WaferPadFinalData,
                PocketData = waferTest.PocketData,
                WaferTests = [waferTest]
            };
            //this._logger.LogInformation("Created new WaferTestLog {WaferId}",waferId);
            await this._waferTestLogCollection.InsertOneAsync(newLog);
            return;
        }

        foreach (var initTest in waferTest.WaferPadInitialData) {
            waferTestLog.WaferPadInitialData[initTest.Key] = initTest.Value;
        }
        
        foreach (var finalTest in waferTest.WaferPadFinalData) {
            waferTestLog.WaferPadFinalData[finalTest.Key]=finalTest.Value;    
        }

        foreach (var pocketData in waferTest.PocketData) {
            waferTestLog.PocketData[pocketData.Key]=pocketData.Value;
        }
        
        var updateTestList=Builders<WaferTestLog>.Update
            .Set(e=>e.WaferPadInitialData,waferTestLog.WaferPadInitialData)
            .Set(e=>e.WaferPadFinalData,waferTestLog.WaferPadFinalData)
            .Set(e=>e.PocketData,waferTestLog.PocketData)
            .Push(e => e.WaferTests, waferTest);
        var filterTestList=Builders<WaferTestLog>.Filter.Eq(e=>e.WaferId,waferId);
        //this._logger.LogInformation("Updated WaferTestLog {WaferId}",waferId);
        await this._waferTestLogCollection.UpdateOneAsync(filterTestList, updateTestList);
    }
    public Task<bool> CheckTested(string waferId,string waferPad) {
        return this._waferTestLogCollection.Find(e=>e.WaferId==waferId &&
                                                    e.WaferTests.Any(e=>e.Probe1Pad==waferPad || 
                                                                        e.Probe2Pad==waferPad)).AnyAsync();
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

    public async Task<List<string>> GetWaferBurnInResult(string waferId) {
        
        var waferTestLog = await this._waferTestLogCollection.Find(e => e.WaferId == waferId).FirstOrDefaultAsync();
        if(waferTestLog is null) {
            return new List<string>();
        }
        List<string> result=new List<string>();
        List<string> finalResult=new List<string>();
        List<string> pocketResult=new List<string>();
        foreach(var pad in _pads) {
            if (waferTestLog.WaferPadInitialData.ContainsKey(pad)) {
                result.Add(waferTestLog.WaferPadInitialData[pad].Voltage.ToString("F", CultureInfo.InvariantCulture));
                result.Add(waferTestLog.WaferPadInitialData[pad].Current.ToString("F", CultureInfo.InvariantCulture));
            } else {
                result.Add("0.00");
                result.Add("0.00");
            }
            if (waferTestLog.WaferPadFinalData.ContainsKey(pad)) {
                finalResult.Add(waferTestLog.WaferPadFinalData[pad].Voltage.ToString("F", CultureInfo.InvariantCulture));
                finalResult.Add(waferTestLog.WaferPadFinalData[pad].Current.ToString("F", CultureInfo.InvariantCulture));
            } else {
                finalResult.Add("0.00");
                finalResult.Add("0.00");
            }
            
            if (waferTestLog.PocketData.ContainsKey(pad)) {
                pocketResult.Add(waferTestLog.PocketData[pad].Pocket.ToString("D", CultureInfo.InvariantCulture));
                pocketResult.Add(waferTestLog.PocketData[pad].SetCurrent.ToString("D", CultureInfo.InvariantCulture));
                pocketResult.Add(waferTestLog.PocketData[pad].SetTemperature.ToString("D", CultureInfo.InvariantCulture));
            } else {
                pocketResult.Add("0");
                pocketResult.Add("0");
                pocketResult.Add("0");
            }
        }
        result.AddRange(finalResult);
        result.AddRange(pocketResult);
        return result;
    }

}
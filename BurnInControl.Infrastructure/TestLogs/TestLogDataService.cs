using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Infrastructure.WaferTestLogs;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.ComDefinitions;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using QuickTest.Data.Models.Wafers.Enums;

namespace BurnInControl.Infrastructure.TestLogs;

public class TestLogDataService {
    private readonly IMongoCollection<BurnInTestLog> _testLogCollection;
    private readonly IMongoCollection<BurnInTestLogEntry> _readingsCollection;
    private readonly StationDataService _stationDataService;
    private readonly WaferTestLogDataService _waferTestLogDataService;
    private readonly ILogger<TestLogDataService> _logger;
    
    public TestLogDataService(IMongoClient client,
        StationDataService stationDataService,
        WaferTestLogDataService waferTestLogDataService,
        ILogger<TestLogDataService> logger,
        IOptions<DatabaseSettings> settings){
        var database = client.GetDatabase(settings.Value.DatabaseName?? "burn_in_db");
        this._testLogCollection=database.GetCollection<BurnInTestLog>(settings.Value.TestLogCollectionName ?? "test_logs");
        this._readingsCollection=database.GetCollection<BurnInTestLogEntry>(settings.Value.TestLogEntryCollection ?? "test_log_entries");
        this._waferTestLogDataService = waferTestLogDataService;
        this._stationDataService = stationDataService;
        this._logger = logger;
    }
    
    public TestLogDataService(IMongoClient client,StationDataService stationDataService){
        var database = client.GetDatabase("burn_in_db");
        this._testLogCollection=database.GetCollection<BurnInTestLog>("test_logs");
        this._readingsCollection=database.GetCollection<BurnInTestLogEntry>("test_log_entries");
        this._stationDataService = stationDataService;
    }

    public async Task<ErrorOr<BurnInTestLog>> LoadSavedLog(ObjectId testId) {
        var log = await this._testLogCollection.Find(e => e._id == testId).Project(e=>new BurnInTestLog() {
                _id=e._id,
                StationId=e.StationId,
                SetCurrent=e.SetCurrent,
                RunTime=e.RunTime,
                StartTime=e.StartTime,
                StopTime=e.StopTime,
                Completed=e.Completed,
                TestSetup = e.TestSetup
            }).FirstOrDefaultAsync();
        return log != null ? log : Error.NotFound(description: "Log not found");
    }
    
    public async Task<List<string>> GetNotCompleted(string stationId) {
        return await this._testLogCollection.Find(e=>e.StationId==stationId && !e.Completed)
            .Project(e=>e._id.ToString())
            .ToListAsync();
    }
    
    public async Task<ErrorOr<BurnInTestLog>> GetTestLogNoReadings(ObjectId? id) {
        if (id == null) {
            return Error.NotFound(description: "BurnInTestLog Not Found, Id is null");
        }
        var log=await this._testLogCollection.Find(e => e._id == id)
            .Project(e=>new BurnInTestLog() {
            _id=e._id,
            StationId=e.StationId,
            SetCurrent=e.SetCurrent,
            RunTime=e.RunTime,
            StartTime=e.StartTime,
            StopTime=e.StopTime,
            Completed=e.Completed,
            TestSetup = e.TestSetup
        }).FirstOrDefaultAsync();
        if (log == null) {
            return Error.NotFound(description: "BurnInTestLog Not Found");
        } else {
            return log;
        }
    }
    
    public async Task<ErrorOr<BurnInTestLog>> GetTestLogWithReadings(ObjectId id) {
        var log=await this._testLogCollection.Find(e => e._id == id).FirstOrDefaultAsync();
        if (log == null) {
            return Error.NotFound(description: "BurnInTestLog Not Found");
        } else {
            return log;
        }
    }
    
    public Task<bool> LogExists(ObjectId id) {
        return this._testLogCollection.Find(e => e._id == id)
            .AnyAsync();
    }
    public async Task<ErrorOr<BurnInTestLog>> StartNew(BurnInTestLog log) {
        log._id = ObjectId.GenerateNewId();
        var result = await this._stationDataService.GetTestConfiguration(log.SetCurrent);
        if (result.IsError) {
            return Error.NotFound(description:$"Test configuration for {log.SetCurrent.Name} not found");
        }
        log.RunTime=result.Value.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        this._logger.LogInformation("Started new test log. Id: {Id}",log._id.ToString());
        return log;
    }
    public async Task<ErrorOr<BurnInTestLog>> StartNewUnknown(BurnInTestLog log,StationCurrent current) {
        var result = await this._stationDataService.GetTestConfiguration(current);
        if (result.IsError) {
            this._logger.LogWarning("Test configuration for {Value} not found",current.Value);
            return Error.NotFound(description:$"Test configuration for {current.Value} not found");
        }
        log.SetCurrent=result.Value.SetCurrent;
        log.RunTime=result.Value.RunTime;
        await this._testLogCollection.InsertOneAsync(log);
        this._logger.LogInformation("Started new unknown test log. Id: {Id}",log._id.ToString());
        return log;
    }
    public async Task<ErrorOr<Deleted>> DeleteTestLog(ObjectId? id) {
        if (id == null) {
            return Error.Unexpected(description:"Failed to delete log. Id is null");
        }
        var deleteResult = await this._testLogCollection.DeleteOneAsync(e => e._id == id);
        if (deleteResult.IsAcknowledged) {
            if(deleteResult.DeletedCount>0){
                this._logger.LogInformation("Log Deleted {Id}",id.ToString());
                return Result.Deleted;
            }
            return Error.Unexpected(description: "Log may not have been deleted");
        }
        return Error.Failure(description: "Failed to delete log");
    }
    public async Task UpdateStartAndRunning(ObjectId id,string stationId,DateTime start,StationSerialData data) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var updateBuilder = Builders<BurnInTestLog>.Update;
        var update = updateBuilder.Set(e => e.StartTime, start);
        BurnInTestLogEntry entry = new BurnInTestLogEntry() {
            _id = ObjectId.GenerateNewId(),
            TestLogId = id,
            TimeStamp = start,
            Reading = data,
        };
        entry.SetReading(data);
        await this._testLogCollection.UpdateOneAsync(filter, update);
        await this._stationDataService.SetRunningTest(stationId, id);
        await this._readingsCollection.InsertOneAsync(entry);
        this._logger.LogInformation("Updated state for Station and Log. Id: {Id}",id.ToString());
    }
    public async Task<ErrorOr<Created>> InsertReading(ObjectId logId,StationSerialData data) {
        BurnInTestLogEntry entry = new BurnInTestLogEntry() {
            _id = ObjectId.GenerateNewId(),
            TestLogId = logId,
            TimeStamp = DateTime.Now,
        };
        entry.SetReading(data);
        await this._readingsCollection.InsertOneAsync(entry);
        return Result.Created;
    }
    public async Task<ErrorOr<Success>> SetCompleted(ObjectId id,string stationId,DateTime stop) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var update = Builders<BurnInTestLog>.Update
            .Set(e => e.StopTime,stop)
            .Set(e=>e.Completed,true);
        var clearResult =await this._stationDataService.ClearRunningTest(stationId);
        var logResult = await this._testLogCollection.UpdateOneAsync(filter, update);
        await this.LogWaferTest(id,30,110);
        if (!clearResult.IsError && logResult.IsAcknowledged) {
            return Result.Success;
        }
        if (!clearResult.IsError && !logResult.IsAcknowledged) {
            return Error.Unexpected(description: "Unknown State. Log may not be finalized with Completed flag and StopTime");
        }
        if (clearResult.IsError && logResult.IsAcknowledged) {
            return Error.Unexpected(description: "Unknown State. Log was finalize but station Running flag was not cleared");
        }
        return Error.Failure(description: "Station Running flag not cleared and Log was not finalized");
    }

    private async Task LogWaferTest(ObjectId testLogId,int initSec,int finalSec) {
        var testLog=await this._testLogCollection.Find(e=>e._id==testLogId)
            .FirstOrDefaultAsync();
        if(testLog != null && testLog.TestSetup?.Count>0) {
            var testSetups = testLog.TestSetup;
            this._logger.LogInformation("Parsing WaferTestLog. InitSec:{Init} FinalSec:{Final}",initSec,finalSec);
            
            var initTestLog = await this._readingsCollection.Find(e => e.TestLogId == testLogId && e.Reading.ElapsedSeconds>=(ulong)initSec)
                .Project(e => e.Reading)
                .FirstOrDefaultAsync();
            var finalTestLog = await this._readingsCollection.Find(e => e.TestLogId == testLogId && e.Reading.ElapsedSeconds>=(ulong)finalSec)
                .Project(e => e.Reading)
                .FirstOrDefaultAsync();
            
            if(initTestLog==null || finalTestLog==null) {
                this._logger.LogWarning("Failed to find initial or final readings for test log {Id}",testLogId.ToString());
                return;
            }
            
            foreach (var pocket in StationPocket.List) {
                var testSetup = testSetups[pocket.Name];
                if (testSetup.Loaded) {
                    WaferTest waferTest = new WaferTest();
                    waferTest.TestId = testLogId;
                    waferTest.Pocket = pocket;
                    waferTest.StartTime = testLog.StartTime;
                    waferTest.StopTime = testLog.StopTime;
                    waferTest.BurnNumber = testSetup.BurnNumber;
                    if (!string.IsNullOrWhiteSpace(testSetup.Probe1Pad)) {
                        var p1Pad = PadLocation.List.FirstOrDefault(e => testSetup.Probe1Pad.Contains(e.Value));
                        if(p1Pad!=null) {
                            waferTest.Probe1Pad = testSetup.Probe1Pad;
                            waferTest.WaferPadInitialData.Add(p1Pad.Value, new WaferPadData() {
                                Voltage =initTestLog.Voltages[(pocket.Value-1)*2],
                                Current =initTestLog.Currents[(pocket.Value-1)*2]
                            });
                            waferTest.WaferPadFinalData.Add(p1Pad.Value, new WaferPadData() {
                                Voltage =finalTestLog.Voltages[(pocket.Value-1)*2],
                                Current =finalTestLog.Currents[(pocket.Value-1)*2]
                            });
                            waferTest.PocketData.Add(p1Pad.Value, new PocketData() {
                                Pocket = pocket.Value,
                                SetCurrent = testLog.SetCurrent.Value,
                                SetTemperature = testLog.SetTemperature
                            });
                        }
                    }
                    
                    if (!string.IsNullOrWhiteSpace(testSetup.Probe2Pad)) {
                        var p2Pad = PadLocation.List.FirstOrDefault(e => testSetup.Probe2Pad.Contains(e.Value));
                        if(p2Pad!=null) {
                            waferTest.Probe2Pad = testSetup.Probe2Pad;
                            waferTest.WaferPadInitialData.Add(p2Pad.Value, new WaferPadData() {
                                Voltage =initTestLog.Voltages[((pocket.Value-1)*2)+1],
                                Current =initTestLog.Currents[((pocket.Value-1)*2)+1]
                            });
                            waferTest.WaferPadFinalData.Add(p2Pad.Value, new WaferPadData() {
                                Voltage =finalTestLog.Voltages[((pocket.Value-1)*2)+1],
                                Current =finalTestLog.Currents[((pocket.Value-1)*2)+1]
                            });
                            waferTest.PocketData.Add(p2Pad.Value, new PocketData() {
                                Pocket = pocket.Value,
                                SetCurrent = testLog.SetCurrent.Value,
                                SetTemperature = testLog.SetTemperature
                            });
                        }
                    }
                    await this._waferTestLogDataService.Insert(testSetup.WaferId, waferTest);
                }
            }
        }
    }
}
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.DataTransfer;
using BurnInControl.Data.BurnInTests.Wafers;
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
    public async Task<ErrorOr<Success>> SetCompleted(ObjectId id,string stationId,DateTime stop,ulong runtime) {
        var filter=Builders<BurnInTestLog>.Filter.Eq(e => e._id,id);
        var update = Builders<BurnInTestLog>.Update
            .Set(e => e.StopTime,stop)
            .Set(e=>e.Completed,true);
        var clearResult =await this._stationDataService.ClearRunningTest(stationId);
        var logResult = await this._testLogCollection.UpdateOneAsync(filter, update);
        await this.LogWaferTest(id,60,(int)((int)runtime-60));
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
    public async Task<(ObjectId Id,Dictionary<string,PocketWaferSetup> Setup)> GetLastTestLog(string stationId) {
        var result = await this._testLogCollection.Find(e => e.StationId == stationId)
            .SortByDescending(e => e.StartTime)
            .Project(e => new { Id = e._id, Setup = e.TestSetup })
            .FirstOrDefaultAsync();
        if (result == null) {
            return new(ObjectId.Empty, []);
        } else {
            return new (result.Id,result.Setup);
        }
    }
    
    public async Task<List<WaferTestReading>> GetTestLogReadings(ObjectId id,StationPocket pocket) {
        var log = await this._testLogCollection.Find(e => e._id == id).FirstOrDefaultAsync();
        if (log != null) {
            var readings = await this._readingsCollection.Find(e => e.TestLogId == id)
                .ToListAsync();
            List<WaferTestReading> waferReadings = new();
            foreach (var reading in readings) {
                var waferReading = new WaferTestReading() {
                    P1Current = reading.PocketData[pocket.Name].Probe1Data.Current,
                    P1Voltage = reading.PocketData[pocket.Name].Probe1Data.Voltage,
                    P1Runtime = reading.PocketData[pocket.Name].Probe1Data.Runtime,
                    P2Current = reading.PocketData[pocket.Name].Probe2Data.Current,
                    P2Voltage = reading.PocketData[pocket.Name].Probe2Data.Voltage,
                    P2Runtime = reading.PocketData[pocket.Name].Probe2Data.Runtime,
                };
                waferReading.Temperature = reading.Reading?.Temperatures[pocket.Value-1] ?? 0;
                waferReadings.Add(waferReading);
            }
            return waferReadings;
        }
        return [];
    }
    
    public async Task<List<StationTestReading>> GetStationTestLogReadings(ObjectId id) {
        var log = await this._testLogCollection.Find(e => e._id == id).FirstOrDefaultAsync();
        if (log != null) {
            var readings = await this._readingsCollection.Find(e => e.TestLogId == id)
                .ToListAsync();
            List<StationTestReading> stationTestReadings = new();
            foreach (var reading in readings) {
                StationTestReading stationTestReading = new StationTestReading() {
                    Temperature = reading.Reading?.TemperatureSetPoint ?? 0,
                    Elapsed = reading.Reading?.ElapsedSeconds ?? 0,
                    SetCurrent = reading.Reading?.CurrentSetPoint ?? 0,
                    V1 = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Voltage,
                    I1 = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Current,
                    Pr1 = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Runtime,
                    P1Okay = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Okay,
                    
                    V2 = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Voltage,
                    I2 = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Current,
                    Pr2 = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Runtime,
                    P2Okay = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Okay,
                    
                    V3 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Voltage,
                    I3 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Current,
                    Pr3 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Runtime,
                    P3Okay = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Okay,
                    
                    V4 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Voltage,
                    I4 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Current,
                    Pr4 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Runtime,
                    P4Okay = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Okay,
                    
                    V5 = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Voltage,
                    I5 = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Current,
                    Pr5 = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Runtime,
                    P5Okay = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Okay,
                    
                    V6 = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Voltage,
                    I6 = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Current,
                    Pr6 = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Runtime,
                    P6Okay = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Okay,
                    T1 = reading.Reading?.Temperatures[0] ?? 0,
                    T2 = reading.Reading?.Temperatures[1] ?? 0,
                    T3 = reading.Reading?.Temperatures[2] ?? 0,
                    H1 = reading.Reading?.HeaterStates[0] ?? false,
                    H2 = reading.Reading?.HeaterStates[1] ?? false,
                    H3 = reading.Reading?.HeaterStates[2] ?? false,
                };
                stationTestReadings.Add(stationTestReading);
            }
            return stationTestReadings;
        }
        return [];
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

    public async Task<IEnumerable<BurnInTestLogDto>> GetRecentStationTests(string stationId,int limit=30) {
        return await this._testLogCollection.Find(e=>e.StationId==stationId)
            .SortByDescending(e=>e.StartTime)
            .Limit(limit)
            .Project(e=>new BurnInTestLogDto() {
                _id=e._id,
                StationId=e.StationId,
                SetCurrent=e.SetCurrent!.Name ?? "Unknown",
                SetTemperature=e.SetTemperature,
                RunTime=e.RunTime,
                StartTime=e.StartTime,
                StopTime=e.StopTime,
                Completed=e.Completed,
                ElapsedTime=e.ElapsedTime,
                LeftPocket=e.TestSetup[StationPocket.LeftPocket.Name].WaferId=="" ? "Empty":e.TestSetup[StationPocket.LeftPocket.Name].WaferId,
                MiddlePocket=e.TestSetup[StationPocket.MiddlePocket.Name].WaferId=="" ? "Empty":e.TestSetup[StationPocket.LeftPocket.Name].WaferId,
                RightPocket=e.TestSetup[StationPocket.RightPocket.Name].WaferId=="" ? "Empty":e.TestSetup[StationPocket.LeftPocket.Name].WaferId
            }).ToListAsync(); 
    }

    public async Task<List<WaferTestResultDto>> GetWaferTestResultsDto(ObjectId id) {
        var testLog = await this._testLogCollection.Find(e => e._id == id)
            .FirstOrDefaultAsync();
        if (testLog != null && testLog.TestSetup?.Count > 0) {
            var testSetups = testLog.TestSetup;
            this._logger.LogInformation("Parsing WaferTestLog. InitSec:{Init} FinalSec:{Final}", 60,
                testLog.RunTime - 60);

            var initTestLog = await this._readingsCollection
                .Find(e => e.TestLogId == id && e.Reading.ElapsedSeconds >= (ulong)60)
                .Project(e => e.Reading)
                .FirstOrDefaultAsync();
            var finalTestLog = await this._readingsCollection.Find(e =>
                    e.TestLogId == id && e.Reading.ElapsedSeconds >= (ulong)testLog.RunTime - 60)
                .Project(e => e.Reading)
                .FirstOrDefaultAsync();

            if (initTestLog == null || finalTestLog == null) {
                this._logger.LogWarning("Failed to find initial or final readings for test log {Id}", id.ToString());
                return [];
            }
            List<WaferTestResultDto> waferTests = new();
            foreach (var pocket in StationPocket.List) {
                var testSetup = testSetups[pocket.Name];
                if (testSetup.Loaded) {
                    WaferTestResultDto waferTest = new WaferTestResultDto { 
                        TestLogId = id, 
                        Pocket = pocket.Name,
                        WaferId = testSetup.WaferId};
                    if (!string.IsNullOrWhiteSpace(testSetup.Probe1Pad)) {
                        var p1Pad = PadLocation.List.FirstOrDefault(e => testSetup.Probe1Pad.Contains(e.Value));
                        if (p1Pad != null) {
                            WaferProbeData probe1Data = new WaferProbeData() {
                                PadId = testSetup.Probe1Pad,
                                RunTime = finalTestLog.ProbeRuntimes[(pocket.Value - 1) * 2],
                                Okay = finalTestLog.ProbeRunTimeOkay[(pocket.Value - 1) * 2],
                                InitVoltage = initTestLog.Voltages[(pocket.Value - 1) * 2],
                                FinalVoltage = finalTestLog.Voltages[(pocket.Value - 1) * 2],
                                InitCurrent = initTestLog.Currents[(pocket.Value - 1) * 2],
                                FinalCurrent = finalTestLog.Currents[(pocket.Value - 1) * 2]
                            };
                            waferTest.Probe1Data = probe1Data;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(testSetup.Probe2Pad)) {
                        var p2Pad = PadLocation.List.FirstOrDefault(e => testSetup.Probe2Pad.Contains(e.Value));
                        if (p2Pad != null) {
                            WaferProbeData probe2Data = new WaferProbeData() {
                                PadId = testSetup.Probe2Pad,
                                RunTime = finalTestLog.ProbeRuntimes[((pocket.Value - 1) * 2) + 1],
                                Okay = finalTestLog.ProbeRunTimeOkay[((pocket.Value - 1) * 2) + 1],
                                InitVoltage = initTestLog.Voltages[((pocket.Value - 1) * 2) + 1],
                                FinalVoltage = finalTestLog.Voltages[((pocket.Value - 1) * 2) + 1],
                                InitCurrent = initTestLog.Currents[((pocket.Value - 1) * 2) + 1],
                                FinalCurrent = finalTestLog.Currents[((pocket.Value - 1) * 2) + 1],
                            };
                            waferTest.Probe2Data = probe2Data;
                        }
                    }
                    waferTests.Add(waferTest);
                }
            }
            return waferTests;
        }
        return [];
    }
}
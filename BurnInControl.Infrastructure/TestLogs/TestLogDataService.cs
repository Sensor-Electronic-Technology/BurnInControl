﻿using System.Globalization;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.DataTransfer;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Infrastructure.WaferTestLogs;
using BurnInControl.Shared;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.Extensions;
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
    private static List<string> _pads = [PadLocation.PadLocationA.Value,PadLocation.PadLocationB.Value,
        PadLocation.PadLocationC.Value, PadLocation.PadLocationR.Value,
        PadLocation.PadLocationL.Value,PadLocation.PadLocationT.Value,
        PadLocation.PadLocationG.Value];
    
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
    
    public async Task<List<StationTestReading>> GetStationTestLogReadings(ObjectId id,int n) {
        var log = await this._testLogCollection.Find(e => e._id == id).FirstOrDefaultAsync();
        if (log != null) {
            var readings = await this._readingsCollection.Find(e => e.TestLogId == id)
                .ToListAsync();
            List<StationTestReading> stationTestReadings = new();
            foreach (var reading in readings.GetNth(n)) {
                StationTestReading stationTestReading = new StationTestReading() {
                    Temperature = reading.Reading?.TemperatureSetPoint ?? 0,
                    Elapsed = reading.Reading!=null ? Math.Round((double)reading.Reading.ElapsedSeconds/3600.00,2) : 0,
                    SetCurrent = reading.Reading?.CurrentSetPoint ?? 0,
                    V1 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Voltage,2),
                    I1 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Current,2),
                    Pr1 = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Runtime,
                    P1Okay = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Okay,
                    
                    V2 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Voltage,2),
                    I2 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Current,2),
                    Pr2 = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Runtime,
                    P2Okay = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Okay,
                    
                    V3 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Voltage,2),
                    I3 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Current,2),
                    Pr3 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Runtime,
                    P3Okay = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Okay,
                    
                    V4 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Voltage,2),
                    I4 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Current,2),
                    Pr4 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Runtime,
                    P4Okay = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Okay,
                    
                    V5 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Voltage,2),
                    I5 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Current,2),
                    Pr5 = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Runtime,
                    P5Okay = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Okay,
                    
                    V6 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Voltage,2),
                    I6 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Current,2),
                    Pr6 = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Runtime,
                    P6Okay = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Okay,
                    T1 =Math.Round( reading.Reading?.Temperatures[0] ?? 0,2),
                    T2 = Math.Round(reading.Reading?.Temperatures[1] ?? 0,2),
                    T3 = Math.Round(reading.Reading?.Temperatures[2] ?? 0,2),
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
    
    public async Task<(string LeftWaferId,string MiddleWaferId,string RightWaferId,List<StationTestReading> readings)> GetWaferTestLogReadings(ObjectId id,int n) {
        var log = await this._testLogCollection.Find(e => e._id == id).FirstOrDefaultAsync();
        string lWaferId, rWaferId, mWaferId;
        lWaferId=log.TestSetup[StationPocket.LeftPocket.Name].WaferId;
        mWaferId=log.TestSetup[StationPocket.MiddlePocket.Name].WaferId;
        rWaferId=log.TestSetup[StationPocket.RightPocket.Name].WaferId;
        if (log != null) {
            var readings = await this._readingsCollection.Find(e => e.TestLogId == id)
                .ToListAsync();
            List<StationTestReading> stationTestReadings = new();
            foreach (var reading in readings.GetNth(n)) {
                StationTestReading stationTestReading = new StationTestReading() {
                    Temperature = reading.Reading?.TemperatureSetPoint ?? 0,
                    Elapsed = reading.Reading!=null ? Math.Round((double)reading.Reading.ElapsedSeconds/3600.00,2) : 0,
                    SetCurrent = reading.Reading?.CurrentSetPoint ?? 0,
                    V1 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Voltage,2),
                    I1 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Current,2),
                    Pr1 = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Runtime,
                    P1Okay = reading.PocketData[StationPocket.LeftPocket.Name].Probe1Data.Okay,
                    
                    V2 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Voltage,2),
                    I2 = Math.Round(reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Current,2),
                    Pr2 = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Runtime,
                    P2Okay = reading.PocketData[StationPocket.LeftPocket.Name].Probe2Data.Okay,
                    
                    V3 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Voltage,2),
                    I3 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Current,2),
                    Pr3 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Runtime,
                    P3Okay = reading.PocketData[StationPocket.MiddlePocket.Name].Probe1Data.Okay,
                    
                    V4 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Voltage,2),
                    I4 = Math.Round(reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Current,2),
                    Pr4 = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Runtime,
                    P4Okay = reading.PocketData[StationPocket.MiddlePocket.Name].Probe2Data.Okay,
                    
                    V5 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Voltage,2),
                    I5 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Current,2),
                    Pr5 = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Runtime,
                    P5Okay = reading.PocketData[StationPocket.RightPocket.Name].Probe1Data.Okay,
                    
                    V6 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Voltage,2),
                    I6 = Math.Round(reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Current,2),
                    Pr6 = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Runtime,
                    P6Okay = reading.PocketData[StationPocket.RightPocket.Name].Probe2Data.Okay,
                    T1 =Math.Round( reading.Reading?.Temperatures[0] ?? 0,2),
                    T2 = Math.Round(reading.Reading?.Temperatures[1] ?? 0,2),
                    T3 = Math.Round(reading.Reading?.Temperatures[2] ?? 0,2),
                    H1 = reading.Reading?.HeaterStates[0] ?? false,
                    H2 = reading.Reading?.HeaterStates[1] ?? false,
                    H3 = reading.Reading?.HeaterStates[2] ?? false,
                };
                stationTestReadings.Add(stationTestReading);
            }
            return (lWaferId,mWaferId,rWaferId,stationTestReadings);
        }
        return ("","","",[]);
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

    public async Task<IEnumerable<BurnInTestLogDto>> GetRecentStationTests(string stationId,TestCountType countType) {
        int limit = countType switch {
            TestCountType.Last60 => 60,
            TestCountType.Last100 => 100,
            TestCountType.Last200 => 200,
            _ => 0
        };
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
                MiddlePocket=e.TestSetup[StationPocket.MiddlePocket.Name].WaferId=="" ? "Empty":e.TestSetup[StationPocket.MiddlePocket.Name].WaferId,
                RightPocket=e.TestSetup[StationPocket.RightPocket.Name].WaferId=="" ? "Empty":e.TestSetup[StationPocket.RightPocket.Name].WaferId
            }).ToListAsync(); 
    }
    
    public async Task<IEnumerable<BurnInTestLogDto>> GetRecentDays(int days) {
        return await this._testLogCollection.Find(e=>e.StartTime>=DateTime.Now.AddDays(-days))
            .SortByDescending(e=>e.StartTime)
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
                MiddlePocket=e.TestSetup[StationPocket.MiddlePocket.Name].WaferId=="" ? "Empty":e.TestSetup[StationPocket.MiddlePocket.Name].WaferId,
                RightPocket=e.TestSetup[StationPocket.RightPocket.Name].WaferId=="" ? "Empty":e.TestSetup[StationPocket.RightPocket.Name].WaferId
            }).ToListAsync(); 
    }

    public async Task<IEnumerable<WaferTestDto>?> GetWaferTests(string waferId) {
        var tests = await this._waferTestLogDataService.GetWaferTests(waferId);
        if (tests == null) {
            return [];
        }
        foreach (var test in tests) {
            var stationId=await this._testLogCollection.Find(e => e._id == test.TestId)
                .Project(e => e.StationId)
                .FirstOrDefaultAsync();
            test.StationId=stationId ?? "S99";
        }
        return tests;
    }
    
    public async Task<List<string>?> GetRecentWaferList(int days) {
        return await this._waferTestLogDataService.GetRecentWaferList(days);
    }
    
    public async Task<List<WaferTestResultDto>> GetWaferTestResultsDto(ObjectId id) {
        var testLog = await this._testLogCollection.Find(e => e._id == id)
            .FirstOrDefaultAsync();
        if (testLog != null && testLog.TestSetup?.Count > 0) {
            var testSetups = testLog.TestSetup;
            this._logger.LogInformation("Parsing WaferTestLog. InitSec:{Init} FinalSec:{Final}", 60,
                testLog.RunTime - 60);

            var initTestLog = await this._readingsCollection
                .Find(e => e.TestLogId == id && e.Reading.ElapsedSeconds >= (ulong)10)
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
    
    public async Task<List<string>> GetExcelBurnInResult(string waferId) {
        var waferTestLog=await this.GetWaferTestResultExcel(waferId);
        if (waferTestLog == null) {
            this._logger.LogWarning("WaferId {waferId} not found,returning empty result",waferId);
            return [];
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
                finalResult.Add(waferTestLog.WaferPadFinalData[pad].RunTime.ToString("D", CultureInfo.InvariantCulture));
                finalResult.Add(waferTestLog.WaferPadFinalData[pad].Okay ? "Okay" : "PFail");
            } else {
                finalResult.Add("0.00");
                finalResult.Add("0.00");
            }
            
            if (waferTestLog.PocketData.ContainsKey(pad)) {
                pocketResult.Add(waferTestLog.PocketData[pad].StationId);
                pocketResult.Add(waferTestLog.PocketData[pad].PadId);
                pocketResult.Add(waferTestLog.PocketData[pad].Pocket.ToString("D", CultureInfo.InvariantCulture));
                pocketResult.Add(waferTestLog.PocketData[pad].SetCurrent.ToString("D", CultureInfo.InvariantCulture));
                pocketResult.Add(waferTestLog.PocketData[pad].SetTemperature.ToString("D", CultureInfo.InvariantCulture));
            } else {
                pocketResult.Add("0");
                pocketResult.Add("0");
                pocketResult.Add("0");
                pocketResult.Add("0");
            }
        }
        result.AddRange(finalResult);
        result.AddRange(pocketResult);

        /*string pad1 = waferResult.Probe1Data?.PadId ?? "";
        string pad2 = waferResult.Probe2Data?.PadId ?? "";
        foreach (var pad in _pads) {
            if (pad1.Contains(pad) || pad2.Contains(pad)) {
                if (pad1.Contains(pad)) {
                    result.Add(waferResult.Probe1Data.InitVoltage.ToString("F", CultureInfo.InvariantCulture));
                    result.Add(waferResult.Probe1Data.InitCurrent.ToString("F", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe1Data.FinalVoltage.ToString("F", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe1Data.FinalCurrent.ToString("F", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe1Data.RunTime.ToString("D", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe1Data.Okay ? "Okay" : "PFail");
                    pocketResult.Add(waferResult.Pocket.ToString("D", CultureInfo.InvariantCulture));
                    pocketResult.Add(waferResult.SetCurrent.ToString("F", CultureInfo.InvariantCulture));
                    pocketResult.Add(waferResult.SetTemperature.ToString("D", CultureInfo.InvariantCulture));
                }else if (pad2.Contains(pad)) {
                    result.Add(waferResult.Probe2Data.InitVoltage.ToString("F", CultureInfo.InvariantCulture));
                    result.Add(waferResult.Probe2Data.InitCurrent.ToString("F", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe2Data.FinalVoltage.ToString("F", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe2Data.FinalCurrent.ToString("F", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe2Data.RunTime.ToString("D", CultureInfo.InvariantCulture));
                    finalResult.Add(waferResult.Probe2Data.Okay ? "Okay" : "PFail");
                    pocketResult.Add(waferResult.Pocket.ToString("D", CultureInfo.InvariantCulture));
                    pocketResult.Add(waferResult.SetCurrent.ToString("F", CultureInfo.InvariantCulture));
                    pocketResult.Add(waferResult.SetTemperature.ToString("D", CultureInfo.InvariantCulture));
                }
            } else {
                result.Add("0.00");   //init voltage
                result.Add("0.00");   //init current
                finalResult.Add("0.00");//final voltage
                finalResult.Add("0.00");//final current
                pocketResult.Add("0");//Pocket
                pocketResult.Add("0");//Set Current
                pocketResult.Add("0");//Set Temperature
            }
        }
        result.AddRange(finalResult);
        result.AddRange(pocketResult);*/
        return result;
    }
    
    public async Task<WaferTestLogV2?> GetWaferTestResultExcel(string waferId) {
        var waferTests = await this.GetWaferTests(waferId);
        if(waferTests==null) {
            return null;
        }

        waferTests = waferTests.ToList();
        if(waferTests.Any()==false) {
            return null;
        }
        
        WaferTestLogV2 waferTestLog = new WaferTestLogV2();
        waferTestLog.WaferId = waferId;
        waferTestLog.WaferPadInitialData = new Dictionary<string, WaferPadData>();
        waferTestLog.WaferPadFinalData = new Dictionary<string, WaferPadDataV2>();
        waferTestLog.PocketData = new Dictionary<string, PocketDataV2>();
        
        foreach (var waferTest in waferTests.ToList()) {
            var testLog=await this._testLogCollection.Find(e => e._id == waferTest.TestId).FirstOrDefaultAsync();
            
            if (testLog != null && testLog.TestSetup?.Count > 0) {
                var testSetups = testLog.TestSetup;
                this._logger.LogInformation("Parsing WaferTestLog. InitSec:{Init} FinalSec:{Final}", 60,
                    testLog.RunTime - 60);

                var initTestLog = await this._readingsCollection
                    .Find(e => e.TestLogId == testLog._id && e.Reading.ElapsedSeconds >= (ulong)60)
                    .Project(e => e.Reading)
                    .FirstOrDefaultAsync();
                
                var finalTestLog = await this._readingsCollection.Find(e =>
                        e.TestLogId == testLog._id && e.Reading.ElapsedSeconds >= (ulong)testLog.RunTime - 60)
                    .Project(e => e.Reading)
                    .FirstOrDefaultAsync();
                
                if (initTestLog == null || finalTestLog == null) {
                    this._logger.LogError("Failed to find initial or final readings for test log {Id}", testLog._id.ToString());
                    continue;
                }
                
                var pocketKey = testSetups.FirstOrDefault(e => e.Value.WaferId == waferId).Key;
                if (string.IsNullOrEmpty(pocketKey)) {
                    this._logger.LogError("Failed to find pocket for wafer {WaferId}", waferId);
                    continue;
                }

                if (StationPocket.TryFromName(pocketKey, out var pocket)) {
                    var testSetup = testSetups[pocket.Name];
                    if (testSetup.Loaded) {
                        if (!string.IsNullOrWhiteSpace(testSetup.Probe1Pad)) {
                            var p1Pad = PadLocation.List.FirstOrDefault(e => testSetup.Probe1Pad.Contains(e.Value));
                            if (p1Pad != null) {
                                WaferPadDataV2 pad1FinalData = new WaferPadDataV2() {
                                    Voltage = finalTestLog.Voltages[(pocket.Value - 1) * 2],
                                    Current = finalTestLog.Currents[(pocket.Value - 1) * 2],
                                    RunTime = finalTestLog.ProbeRuntimes[(pocket.Value - 1) * 2],
                                    Okay = finalTestLog.ProbeRunTimeOkay[(pocket.Value - 1) * 2]
                                    
                                };
                                WaferPadData pad1InitData = new WaferPadData() {
                                    Voltage = initTestLog.Voltages[(pocket.Value - 1) * 2],
                                    Current = initTestLog.Currents[(pocket.Value - 1) * 2],
                                };
                                PocketDataV2 pad1Data = new PocketDataV2() {
                                    Pocket = pocket.Value,
                                    SetCurrent = testLog.SetCurrent?.Value ?? 0,
                                    SetTemperature = testLog.SetTemperature,
                                    StationId = testLog.StationId,
                                    PadId = testSetup.Probe1Pad
                                };
                                waferTestLog.WaferPadInitialData[p1Pad]=pad1InitData;
                                waferTestLog.WaferPadFinalData[p1Pad]=pad1FinalData;
                                waferTestLog.PocketData[p1Pad]=pad1Data;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(testSetup.Probe2Pad)) {
                            var p2Pad = PadLocation.List.FirstOrDefault(e => testSetup.Probe2Pad.Contains(e.Value));
                            if (p2Pad != null) {
                                WaferPadDataV2 pad2FinalData = new WaferPadDataV2() {
                                    Voltage = finalTestLog.Voltages[((pocket.Value - 1) * 2) + 1],
                                    Current = finalTestLog.Currents[((pocket.Value - 1) * 2) + 1],
                                    RunTime = finalTestLog.ProbeRuntimes[((pocket.Value - 1) * 2) + 1],
                                    Okay = finalTestLog.ProbeRunTimeOkay[((pocket.Value - 1) * 2) + 1]
                                    
                                };
                                WaferPadData pad2InitData = new WaferPadData() {
                                    Voltage = initTestLog.Voltages[((pocket.Value - 1) * 2) + 1],
                                    Current = initTestLog.Currents[((pocket.Value - 1) * 2) + 1],
                                };
                                PocketDataV2 pad2Data = new PocketDataV2() {
                                    Pocket = pocket.Value,
                                    SetCurrent = testLog.SetCurrent?.Value ?? 0,
                                    SetTemperature = testLog.SetTemperature,
                                    StationId = testLog.StationId,
                                    PadId = testSetup.Probe2Pad
                                };
                                waferTestLog.WaferPadInitialData[p2Pad]=pad2InitData;
                                waferTestLog.WaferPadFinalData[p2Pad]=pad2FinalData;
                                waferTestLog.PocketData[p2Pad]=pad2Data;
                            }
                        }

                        return waferTestLog;
                    }
                }
            }
        }
        return null;
    }
}
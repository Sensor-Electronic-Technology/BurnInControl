using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Infrastructure.ControllerTestState;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Shared;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using StationService.Infrastructure.Hub;

namespace StationService.Infrastructure.TestLogs;

public class TestService:ITestService {
    private readonly TestLogDataService _testLogDataService;
    private readonly SavedStateDataService _savedStateDataService;
    private StationSerialData _latestData=new StationSerialData();
    private BurnInTestLog _runningTest=new BurnInTestLog();
    private SavedStateLog _savedStateLog=new SavedStateLog();
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly IMediator _mediator;
    private string? _stationId;
    private DateTime _lastLog;
    private DateTime _timeStopped;
    private readonly TimeSpan _interval=TimeSpan.FromSeconds(60);
    private readonly ILogger<TestService> _logger;
    private readonly string _path = "/test-logs/";
    private string _filePath = string.Empty;
    private bool _running=false, _paused=false;
    private bool _loggingEnabled = false;
    private bool _testSetupComplete = false;
    private bool _first = false;
    private bool _testRequested = false;
    private bool _waitingForComplete = false;
    
    private ulong _runTime = 0ul;
    private bool _stopped=false;
    private bool _databaseLogEnabled;
    public bool IsRunning => this._running;

    public TestService(TestLogDataService testLogDataService,
        IHubContext<StationHub, IStationHub> hubContext,
        IMediator mediator,SavedStateDataService savedStateDataService,
        IConfiguration configuration,
        ILogger<TestService> logger) {
        this._logger = logger;
        this._mediator = mediator;
        this._testLogDataService = testLogDataService;
        this._hubContext = hubContext;
        this._savedStateDataService = savedStateDataService;
        this._stationId=configuration["StationId"] ?? "S99";
    }

    private void Reset() {
        this._first = false;
        this._loggingEnabled = false;
        this._paused = false;
        this._running = false;
        this._testRequested = false;
        this._testSetupComplete = false;
        this._waitingForComplete = false;
        this._stopped = false;
        this._databaseLogEnabled = false;
        this._runTime = 0ul;
        /*this._startTime = DateTime.MinValue;*/
        this._lastLog = DateTime.MinValue;
        /*this._targetFinish = DateTime.MinValue;
        this._timePaused = DateTime.MinValue;*/
        this._timeStopped = DateTime.MinValue;
        this._runningTest.Reset();
        this._savedStateLog.Reset();
    }
    public async Task SetupTest(TestSetupTransport testSetup) {
        if (!this.IsRunning) {
            this._runningTest.Reset();
            this._runningTest.StartNew(testSetup.WaferSetups,
                testSetup.SetTemperature,
                testSetup.SetCurrent);
            this._runningTest.StationId = this._stationId ?? "S99";
            this._logger.LogInformation("TestTransport: Temp: {SetTemp} " +
                                        "Current:{Current}",testSetup.SetTemperature,testSetup.SetCurrent.Name);
            var result = await this._testLogDataService.StartNew(this._runningTest);
            if (!result.IsError) {
                this._testSetupComplete = true;
                this._runningTest = result.Value;
                await this._mediator.Send(new SendTestIdCommand() {
                    TestId=this._runningTest._id.ToString()
                });
                await this._hubContext.Clients.All.OnTestSetup(true, "Test Setup Complete, " +
                                                                     "start the test when ready");
            } else {
                this._testSetupComplete = false;
                await this._hubContext.Clients.All.OnTestSetup(false,$"Test setup failed. " +
                                                                     $"Internal Error: " +
                                                                     $"{result.FirstError.Description}");
                this._logger.LogError("Test setup failed. Internal " +
                                      "Error: {ErrMessage}",result.FirstError.Description);
            }
        } else {
            await this._hubContext.Clients.All.OnTestSetup(false,"Test is already running, " +
                                                                 "please reset controller or wait for current " +
                                                                 "test to complete before starting new test");
        }
    }
    public long RemainingTimeSecs() {
        if(!this._running) {
            return 0;
        }
        return this._runningTest.RemainingTimeSecs();
    }
    public async Task Start(bool success, string? message) {
        if (success) {
            this._running = true;
            this._paused = false;
            this._loggingEnabled = true;
            this._databaseLogEnabled = true;
            this._first = true;
            await this._hubContext.Clients.All.OnTestStarted($"Test Started Successfully, Message: {message ?? "No Message"}");
        } else {
            this._running = false;
            this._paused = false;
            this._loggingEnabled = false;
            this._databaseLogEnabled = false;
            await this._hubContext.Clients.All.OnTestStartedFailed($"Test failed to start, " +
                                                                   $"Message: {message ?? "Unknown Error"}");
        }
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestStartAck });
    }
    public async Task StartFrom(ControllerSavedState savedState) {
        if (this.IsRunning) {
           if(this._savedStateLog.SavedState.TestId==savedState.TestId) {
               this._logger.LogInformation("Received test state,Test already setup and running");
               await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
                   Success = true,
                   Message = "Received test state,Test already setup and running",
                   PocketWaferSetups = this._runningTest.TestSetup.Select(e => e.Value).ToList(),
                   SetTemperature = this._runningTest.SetTemperature,
                   SetCurrent = this._runningTest.SetCurrent
               });
           } else {
               this._logger.LogCritical("Received test state. Test already running but the received " +
                                        "testId{RTestId} does not match the running testId{TestId}",
                   savedState.TestId,this._savedStateLog.SavedState.TestId ?? "Null");
               //TODO: Handle sending message to UI
           }
        } else {
            if (!string.IsNullOrWhiteSpace(savedState.TestId)) {
                var savedStateResult=await this._savedStateDataService.GetSavedState(savedState.TestId);
                if(!savedStateResult.IsError) {
                    this._savedStateLog=savedStateResult.Value;
                    if (!await LoadLog(this._savedStateLog.LogId)) {
                        this._logger.LogWarning("Failed to load saved state for TestId: {TestId} attempting to load log",
                            this._savedStateLog.SavedState.TestId ?? " NULL");
                        if (ObjectId.TryParse(savedState.TestId, out var id)) {
                            await this.LoadLog(id);
                        } else {
                            this._logger.LogWarning("Failed to load saved state and log.  TestId was empty");
                        }
                    }
                } else {
                    this._logger.LogWarning("Failed to load saved state for TestId: {TestId} attempting to load log",
                        this._savedStateLog.SavedState.TestId ?? " NULL");
                    if (ObjectId.TryParse(savedState.TestId, out var id)) {
                        await this.LoadLog(id);
                    } else {
                        this._logger.LogWarning("Failed to load saved state and log. Error while parsing TestId." +
                                                "Test will continue running with file logging only");
                    }
                }   
            } else {
                this._running = true;
                this._databaseLogEnabled = false;
                this._loggingEnabled = true;
                this._paused = false;
                this._logger.LogWarning("Failed to load saved state.  TestId was empty, " +
                                        "test will continue logging with file logging only");
                await this._hubContext.Clients.All.OnLoadFromSavedStateError(
                    "Failed to load saved state. TestId was empty, " +
                    "test will continue logging with file logging only");

            }
        }
        this._testRequested = false;
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestStartAck });
    }
    private async Task<bool> LoadLog(ObjectId? logId) {
        var logResult = await this._testLogDataService.GetTestLogNoReadings(logId);
        if (!logResult.IsError) {
            this._runningTest = logResult.Value;
            this._running = true;
            this._paused = false;
            this._loggingEnabled = true;
            this._first = false;
            this._databaseLogEnabled = true;
            await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
                Success = true,
                Message = "Test loaded from saved state",
                PocketWaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
            return true;
        } else {
            this._running = true;
            this._paused = false;
            this._loggingEnabled = true;
            this._first = false;
            this._databaseLogEnabled = false;
            /*wait this.StartFromUnknown(savedState);*/
            this._logger.LogError("Failed to load log for TestId: {TestId}, Error: {Error}",
                logId?.ToString() ?? " NULL",logResult.FirstError.Description);
            return false;
        }
    }
    public async Task LoadState(ObjectId savedStateId) {
        var savedStateResult=await this._savedStateDataService.GetSavedState(logId:savedStateId);
        if (savedStateResult.IsError) {
            this._savedStateLog=savedStateResult.Value;
            var logResult = await this._testLogDataService.GetTestLogNoReadings(this._savedStateLog.LogId);
            if (!logResult.IsError) {
                this._runningTest = logResult.Value;
                this._running = true;
                this._paused = false;
                this._loggingEnabled = true;
                this._databaseLogEnabled = true;
                this._first = false;
                await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
                    Success = true,
                    Message = "Test loaded from saved state",
                    PocketWaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                    SetTemperature = this._runningTest.SetTemperature,
                    SetCurrent = this._runningTest.SetCurrent
                });
            } else {
                this._databaseLogEnabled = false;
                await this._hubContext.Clients.All.OnLoadFromSavedStateError("Failed to load test from saved state.  " +
                                                                             "Saved state exists but the log is missing.\n" +
                                                                             "Test will continue running with file logging only");
            }
        } else {
            this._databaseLogEnabled = false;
            await this._hubContext.Clients.All.OnLoadFromSavedStateError("Failed to load saved state. Saved state not found");
        }
    }
    public async Task StopAndSaveState() {
        if (this._running) {
            var savedState = new ControllerSavedState(this._latestData) {
                TestId = this._runningTest._id.ToString()
            };
            var result=await this._savedStateDataService.UpdateLog(this._savedStateLog._id,savedState);
            if (!result.IsError) {
                this.Reset();
                await this._hubContext.Clients.All.OnStopAndSaved(true,"State is saved. Resetting controller");
                await this._mediator.Send(new SendStationCommand() {
                    Command = StationCommand.Reset
                });
            } else {
                await this._hubContext.Clients.All.OnStopAndSaved(false,"Test failed to save. Please try again or hard reset." +
                                                                        "Note* you will lose the saved state if you hard reset");
            }
        } else {
            await this._hubContext.Clients.All.OnStopAndSaved(false,"Test is not running, nothing to save");
        }
    }
    public Task SendRunningTest() {
        if (this._running) {
            this._logger.LogInformation("Sending running test.  TestId:{ID}",this._runningTest._id.ToString());
            return this._hubContext.Clients.All.OnRequestRunningTest(new LoadTestSetupTransport() {
                Success = true,
                Message = "Running Test",
                PocketWaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        }else if (this._latestData.Running) {
            this._logger.LogInformation("TestService has no running test.  Requesting running test from controller");
            return this._mediator.Send(new SendStationCommand(){Command = StationCommand.RequestRunningTest});
        } else {
            this._logger.LogInformation("Running test requested.  No test running");
        }
        return Task.CompletedTask;
    }
    private async Task UpdateSavedState(StationSerialData data) {
        this._savedStateLog.SavedState=new ControllerSavedState(data) {
            TestId = this._runningTest._id.ToString()
        };
        var result=await this._savedStateDataService.UpdateLog(this._savedStateLog._id,this._savedStateLog.SavedState);
        if(result.IsError) {
            this._logger.LogWarning("Failed to update saved state for station {StationId}",this._stationId ?? "S99");
        }
    }
    public async Task CompleteTest() {
        if (this._databaseLogEnabled) {
            var result = await this._testLogDataService.SetCompleted(this._runningTest._id,
                this._stationId ?? "S99", DateTime.Now,this._runTime);
            var delStateResult=await this._savedStateDataService.ClearSavedState(id:this._savedStateLog._id);
            if (!result.IsError) {
                await this._hubContext.Clients.All.OnTestCompleted("Test Completed");
            } else {
                await this._hubContext.Clients.All.OnTestCompleted("Test completed but Failed to mark test as completed");
            }
            if (!delStateResult.IsError) {
                this._logger.LogInformation("Cleared saved state");
            } else {
                this._logger.LogWarning("Failed to clear saved state");
            }
        }
        this.Reset();
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestCompleteAck });
    }
    private async Task SaveState(StationSerialData data) {
        var controllerSavedState=new ControllerSavedState(data);
        controllerSavedState.TestId=this._runningTest._id.ToString();
        this._savedStateLog=new SavedStateLog() {
            _id = ObjectId.GenerateNewId(),
            TimeStamp=DateTime.Now,
            StationId=this._stationId ?? "S99",
            LogId=this._runningTest._id,
        };
        this._savedStateLog.SavedState=controllerSavedState;
        var result=await this._savedStateDataService.SaveState(this._savedStateLog);
        if (!result.IsError) {
            this._savedStateLog = result.Value;
            this._logger.LogInformation("First state saved for test {TestId}",this._savedStateLog.SavedState.TestId ?? " NULL");
            return;
        }
        this._logger.LogWarning("Failed to log saved state.  Message: {Message}",result.FirstError.Description);
    }
    private async Task StartLog(StationSerialData data) {
        for (int i = 0; i < ControllerHardwareConstants.PROBE_COUNT; i++) {
            data.Currents[i]=Math.Round(data.Currents[i],2);
            data.Voltages[i]=Math.Round(data.Voltages[i],2);
            if (i < ControllerHardwareConstants.HEATER_COUNT) {
                data.Temperatures[i]=Math.Round(data.Temperatures[i],2);
            }
        }

        if (this._databaseLogEnabled) {
            await this.SaveState(data);
            await this._testLogDataService.UpdateStartAndRunning(this._runningTest._id,
                this._stationId ?? "S99",DateTime.Now,data);
        }
        await this.LogFile(data, true);
    }
    private async Task UpdateLogs(StationSerialData data) {
        for (int i = 0; i < ControllerHardwareConstants.PROBE_COUNT; i++) {
            data.Currents[i]=Math.Round(data.Currents[i],2);
            data.Voltages[i]=Math.Round(data.Voltages[i],2);
            if (i < ControllerHardwareConstants.HEATER_COUNT) {
                data.Temperatures[i]=Math.Round(data.Temperatures[i],2);
            }
        }
        if (this._databaseLogEnabled) {
            await this._testLogDataService.InsertReading(this._runningTest._id,data);
            await this.UpdateSavedState(data);
        }
        await this.LogFile(data, false);

    }
    public async Task Stop() {
        this._logger.LogInformation("Deleting test log and saved state");
        ObjectId? savedStateId = this._savedStateLog._id;
        ObjectId? logId = this._runningTest._id;
        ObjectId? logId2 = this._savedStateLog.LogId;
        this.Reset();
        this._stopped = true;
        var savedStateResult=await this._savedStateDataService.ClearSavedState(id:savedStateId);
        if(savedStateResult.IsError) {
            this._logger.LogWarning("Failed to clear saved state, Id: {Id}. Trying by log id",savedStateId.ToString());
            savedStateResult=await this._savedStateDataService.ClearSavedState(logId: logId);
            if(savedStateResult.IsError) {
                this._logger.LogWarning("Failed to clear saved state by log id, LogId: {Id}",logId.ToString());
            }
        }
        var response=await this._testLogDataService.DeleteTestLog(logId);
        if (response.IsError) {
            await this._testLogDataService.DeleteTestLog(logId2);
        }
    }
    private void GeneratePath() {
        foreach(var setup in this._runningTest.TestSetup) {
            string waferId = string.IsNullOrWhiteSpace(setup.Value.WaferId) ? "Empty" : setup.Value.WaferId;
            if (setup.Key == StationPocket.LeftPocket.Name) {
                this._filePath += $"{waferId}{setup.Value.Probe1Pad ?? "N"}{setup.Value.Probe2Pad ?? "N"}";
            } else {
                this._filePath += $"_{waferId}{setup.Value.Probe1Pad ?? "N"}{setup.Value.Probe2Pad ?? "N"}";
            }
        }
        this._filePath+=".csv";
    }
    private string GenerateRunTimeString(ulong elapsedSecs) {
        ulong hours=elapsedSecs/3600;
        ulong minutes=(elapsedSecs/60)%60;
        ulong seconds=elapsedSecs%60;
        string runtTimeString = "";
        if ((hours / 10) < 1 || hours==0) {
            runtTimeString="0"+hours+":";
        } else {
            runtTimeString=hours+":";
        }
        
        if ((minutes / 10) < 1 || minutes==0) {
            runtTimeString+="0"+minutes+":";
        } else {
            runtTimeString+=minutes+":";
        }
        
        if ((seconds / 10) < 1 || seconds==0) {
            runtTimeString+="0"+seconds;
        } else {
            runtTimeString+=seconds;
        }
        return runtTimeString;
    }
    private string GenerateLogLine(StationSerialData data) {
        var now = DateTime.Now;
        string logLine=$"{now:d},{now:h:mm:ss tt zz}," +
                       $"{this.GenerateRunTimeString(data.ElapsedSeconds)}," +
                       $"{data.ElapsedSeconds},";
        foreach(var voltage in data.Voltages) {
            logLine+=$"{voltage},";
        }
        foreach(var current in data.Currents) {
            logLine+=$"{current},";
        }
        foreach(var probeRuntime in data.ProbeRuntimes) {
            logLine+=$"{probeRuntime},";
        }
        foreach(var temp in data.Temperatures) {
            logLine+=$"{temp},";
        }
        return logLine;
    }
    private async Task LogFile(StationSerialData data,bool first) {
        /*if (Directory.Exists(this._path)) {*/
            if (first) {
                GeneratePath();
                var header= "Date,System Time,RunTime,Elapsed(secs)," +
                            "V11,V12,V21,V22,V31,V32," +
                            "i11,i12,i21,i22,i31,i32," +
                            "p1,p2,p3,p4,p5,p6," +
                            "Temp1,Temp2,Temp3,CurrentSetPoint(mA)";
                await using StreamWriter stream = File.AppendText(this._filePath);
                await stream.WriteLineAsync(header);
                await stream.WriteLineAsync(this.GenerateLogLine(data));
            } else {
                await using StreamWriter stream = File.AppendText(this._filePath);
                await stream.WriteLineAsync(this.GenerateLogLine(data));
            }
        /*} else {
            this._logger.LogWarning("Failed to log data.  Directory does not exist");
        }*/
    }
    public async Task Log(StationSerialData data) {
        this._latestData = data;
        if (this._loggingEnabled) {
            if (this._first) {
                var now = DateTime.Now;
                this._first = false;
                this._lastLog = now;
                this._runTime = data.RuntimeSeconds;
                await this.StartLog(data);
                
            } else {
                var now = DateTime.Now;
                if (this._running != data.Running) {
                    if (this._running) {
                        this._timeStopped = now;
                        this._waitingForComplete = true;
                    }
                    this._running = data.Running;
                }
                this._paused=data.Paused;
                if ((now - this._lastLog) >= this._interval) {
                    this._lastLog = DateTime.Now;
                    if (!this._paused) {
                        await this.UpdateLogs(data);
                        
                    }
                }
            }
            
        }
    }
}
﻿using System.Text;
using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Infrastructure.ControllerTestState;
using BurnInControl.Infrastructure.TestLogs;
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
    private readonly TimeSpan _interval=TimeSpan.FromSeconds(1);
    private readonly ILogger<TestService> _logger;

    private bool _running=false, _paused=false;
    private bool _loggingEnabled = false;
    private bool _testSetupComplete = false;
    private bool _first = false;
    private bool _testRequested = false;
    private string _path = "";
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
        this._stationId=configuration["StationId"] ?? "S01";
    }

    private void Reset() {
        this._first = false;
        this._loggingEnabled = false;
        this._paused = false;
        this._running = false;
        this._runningTest.Reset();
        this._savedStateLog.Reset();
        this._testSetupComplete = false;
    }
    public async Task SetupTest(TestSetupTransport testSetup) {
        if (!this.IsRunning) {
            this._runningTest.Reset();
            this._runningTest.StartNew(testSetup.WaferSetups,
                testSetup.SetTemperature,
                testSetup.SetCurrent);
            this._runningTest.StationId = this._stationId ?? "S00";
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
            this._first = true;
            await this._hubContext.Clients.All.OnTestStarted($"Test Started Successfully, Message: {message ?? "No Message"}");
        } else {
            await this._hubContext.Clients.All.OnTestStartedFailed($"Test failed to start, " +
                                                                   $"Message: {message ?? "Unknown Error"}");
        }
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestStartAck });
    }
    public async Task StartFrom(ControllerSavedState savedState) {
        this._testRequested = false;
        if (this.IsRunning) {
           if(this._savedStateLog.SavedState.TestId==savedState.TestId) {
               this._logger.LogInformation("Received test state,Test already setup and running");
               await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
                   Success = true,
                   Message = "Received test state,Test already setup and running",
                   WaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                   SetTemperature = this._runningTest.SetTemperature,
                   SetCurrent = this._runningTest.SetCurrent
               });
           } else {
               this._logger.LogCritical("Received test state. Test already running but the received " +
                                        "testId{RTestId} does not match the running testId{TestId}",
                   savedState.TestId,this._savedStateLog.SavedState.TestId ?? "Null");
           }
        } else {
            var savedStateResult=await this._savedStateDataService.GetSavedState(savedState.TestId);
            if(!savedStateResult.IsError) {
                this._savedStateLog=savedStateResult.Value;
                var logResult = await this._testLogDataService.GetTestLogNoReadings(this._savedStateLog.LogId);
                if (!logResult.IsError) {
                    this._runningTest = logResult.Value;
                    this._running = true;
                    this._paused = false;
                    this._loggingEnabled = true;
                    this._first = false;
                    
                    this._logger.LogInformation("Loaded saved state.  TestId: {TestId}",this._savedStateLog.SavedState.TestId ?? " NULL");
                    await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
                        Success = true,
                        Message = "Test loaded from saved state",
                        WaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                        SetTemperature = this._runningTest.SetTemperature,
                        SetCurrent = this._runningTest.SetCurrent
                    });
                } else {
                    await this.StartFromUnknown(savedState);
                }
            } else {
                this._logger.LogInformation("Failed to load saved state for TestId: {TestId} attempting to load log",
                    this._savedStateLog.SavedState.TestId ?? " NULL");
                if(ObjectId.TryParse(savedState.TestId,out var id)) {
                    var logResult = await this._testLogDataService.GetTestLogNoReadings(id);
                    if (!logResult.IsError) {
                        this._runningTest = logResult.Value;
                        this._running = true;
                        this._paused = false;
                        this._loggingEnabled = true;
                        this._first = false;
                        await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
                            Success = true,
                            Message = "Test loaded from saved state",
                            WaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                            SetTemperature = this._runningTest.SetTemperature,
                            SetCurrent = this._runningTest.SetCurrent
                        });
                    } else {
                        //TODO Handle this, temp start unknown and send warning
                        await this.StartFromUnknown(savedState);
                    }
                } else {
                    //TODO Handle this, temp start unknown and send warning
                    await this.StartFromUnknown(savedState);
                }
            }            
        }
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestStartAck });
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
                WaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        }
        this._logger.LogInformation("Running test requested.  No test running");
        return Task.CompletedTask;
    }
    public async Task LoadState(ObjectId savedState) {
        var savedStateResult=await this._savedStateDataService.GetSavedState(logId:savedState);
        if (savedStateResult.IsError) {
            this._savedStateLog=savedStateResult.Value;
            var logResult = await this._testLogDataService.GetTestLogNoReadings(this._savedStateLog.LogId);
            if (!logResult.IsError) {
                this._runningTest = logResult.Value;
                this._running = true;
                this._paused = false;
                this._loggingEnabled = true;
                this._first = false;
                await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
                    Success = true,
                    Message = "Test loaded from saved state",
                    WaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                    SetTemperature = this._runningTest.SetTemperature,
                    SetCurrent = this._runningTest.SetCurrent
                });
            } else {
                await this._hubContext.Clients.All.OnLoadFromSavedStateError("Failed to load test from saved state.  " +
                                                                             "Saved state exists but the log is missing");
            }
        } else {
            await this._hubContext.Clients.All.OnLoadFromSavedStateError("Failed to load saved state. Saved state not found");
        }
    }
    private async Task UpdateSavedState(StationSerialData data) {
        this._savedStateLog.SavedState=new ControllerSavedState(data);
        this._savedStateLog.SavedState.TestId=this._runningTest._id.ToString();
        var result=await this._savedStateDataService.UpdateLog(this._savedStateLog._id,this._savedStateLog.SavedState);
        if(result.IsError) {
            this._logger.LogWarning("Failed to update saved state for station {StationId}",this._stationId ?? "S00");
        }
    }
    private async Task StartFromUnknown(ControllerSavedState savedState) {
        this.CreateUnknownTest(savedState.SetCurrent,savedState.SetTemperature);
        var startResult=await this._testLogDataService.StartNewUnknown(this._runningTest,savedState.SetCurrent);
        if (!startResult.IsError) {
            this._runningTest = startResult.Value;
            this._loggingEnabled = true;
            this._first = false;
            await this._hubContext.Clients.All.OnTestStartedFromUnknown(new LoadTestSetupTransport() {
                Success = true,
                Message = "Saved state was not found.  Started unknown test instead.",
                WaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        } else {
            this._loggingEnabled=false;
            await this._hubContext.Clients.All.OnTestStartedFromUnknown(new LoadTestSetupTransport() {
                Success = false,
                Message = "Saved state was not found and failed to create unknown test. " +
                          "Test will continue to run but no logs will be available.",
                WaferSetups = this._runningTest.TestSetup.Select(e=>e.Value).ToList(),
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        }
    }
    public async Task CompleteTest() {
        var result=await this._testLogDataService.SetCompleted(this._runningTest._id,
            this._stationId ?? "S01",DateTime.Now);
        var delStateResult=await this._savedStateDataService.ClearSavedState(id:this._savedStateLog._id);
        this.Reset();
        if (!result.IsError) {
            await this._hubContext.Clients.All.OnTestCompleted("Test Completed");
        } else {
            await this._hubContext.Clients.All.OnTestCompleted("Error: Failed to mark test as completed");
        }
        if (!delStateResult.IsError) {
            this._logger.LogInformation("Cleared saved state");
        } else {
            this._logger.LogWarning("Failed to clear saved state");
        }
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestCompleteAck });
    }
    private async Task SaveState(StationSerialData data) {
        var controllerSavedState=new ControllerSavedState(data);
        controllerSavedState.TestId=this._runningTest._id.ToString();
        this._savedStateLog=new SavedStateLog() {
            _id = ObjectId.GenerateNewId(),
            TimeStamp=DateTime.Now,
            StationId=this._stationId ?? "S00",
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
    private void CreateUnknownTest(StationCurrent current,int setTemp) {
        BurnInTestLog log=new BurnInTestLog { _id = ObjectId.GenerateNewId() };
        log.CreateUnknown(current,setTemp,this._stationId ?? "S00");
        this._runningTest = log;
    }
    private async Task StartLog(StationSerialData data) {
        await this.SaveState(data);
        await this._testLogDataService.UpdateStartAndRunning(this._runningTest._id,
            this._stationId ?? "S01",DateTime.Now,data);
    }
    private async Task UpdateLogs(StationSerialData data) {
        await this._testLogDataService.InsertReading(this._runningTest._id,data);
        await this.UpdateSavedState(data);
    }
    
    public async Task Stop() {
        if(this.IsRunning) {
            this._logger.LogInformation("Deleting test log and saved state");
            ObjectId? savedStateId = this._savedStateLog._id;
            ObjectId? logId = this._runningTest._id;
            ObjectId? logId2 = this._savedStateLog.LogId;
            this.Reset();
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
    }
    
    private void GeneratePath() {
        this._path = "/test-logs/";
        foreach(var setup in this._runningTest.TestSetup) {
            string waferId = string.IsNullOrWhiteSpace(setup.Value.WaferId) ? "Empty" : setup.Value.WaferId;
            if (setup.Key == StationPocket.LeftPocket.Name) {
                this._path += $"{waferId}{setup.Value.Probe1Pad ?? "N"}{setup.Value.Probe2Pad ?? "N"}";
            } else {
                this._path += $"_{waferId}{setup.Value.Probe1Pad ?? "N"}{setup.Value.Probe2Pad ?? "N"}";
            }
        }
        this._path+=".csv";
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
        if (first) {
            GeneratePath();
            var header= "Date,System Time,RunTime,Elapsed(secs)," +
                        "V11,V12,V21,V22,V31,V32," +
                        "i11,i12,i21,i22,i31,i32," +
                        "p1,p2,p3,p4,p5,p6," +
                        "Temp1,Temp2,Temp3,CurrentSetPoint(mA)";
            await using StreamWriter stream = File.AppendText(this._path);
            await stream.WriteLineAsync(header);
            await stream.WriteLineAsync(this.GenerateLogLine(data));
        } else {
            await using StreamWriter stream = File.AppendText(this._path);
            await stream.WriteLineAsync(this.GenerateLogLine(data));
        }
    }



    public async Task Log(StationSerialData data) {
        this._latestData = data;
        if (!this._running && data.Running && !this._testSetupComplete) {
            if (!this._testRequested) {
                Console.WriteLine("Requesting test");
                this._testRequested = true;
                await this._mediator.Send(new SendStationCommand() { Command = StationCommand.RequestRunningTest });
            }
        }
        if (this._loggingEnabled) {
            if (this._first) {
                this._first = false;
                this._lastLog = DateTime.Now;
                await this.StartLog(data);
                await this.LogFile(data, true);
            } else {
                this._running = data.Running;
                this._paused = data.Paused;
                if ((DateTime.Now - this._lastLog) >= this._interval) {
                    this._lastLog = DateTime.Now;
                    if (!this._paused) {
                        await this.UpdateLogs(data);
                        await this.LogFile(data, false);
                    }
                }
            }
        }
    }
}
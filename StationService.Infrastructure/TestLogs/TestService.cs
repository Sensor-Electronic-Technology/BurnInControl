﻿using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using StationService.Infrastructure.Hub;

namespace StationService.Infrastructure.TestLogs;

public class TestService:ITestService {
    private readonly TestLogDataService _testLogDataService;
    private StationSerialData _latestData=new StationSerialData();
    private BurnInTestLog _runningTest=new BurnInTestLog();
    private IHubContext<StationHub, IStationHub> _hubContext;
    private IMediator _mediator;
    private string? _stationId;
    private DateTime _lastLog;
    private readonly TimeSpan _interval;
    private readonly ILogger<TestService> _logger;
    private List<StationSerialData> _readings=new List<StationSerialData>();

    private bool _running=false, _paused=false;
    private bool _loggingEnabled = false;
    private bool _testSetupComplete = false;
    private bool _first = false;

    public bool IsRunning => this._running;
    
    public TestService(TestLogDataService testLogDataService,
        IHubContext<StationHub, IStationHub> hubContext,
        IMediator mediator,
        ILogger<TestService> logger) {
        this._logger = logger;
        this._mediator = mediator;
        this._testLogDataService = testLogDataService;
        this._hubContext = hubContext;
        this._interval = TimeSpan.FromSeconds(10);
    }
    
    public async Task SetupTest(TestSetupTransport testSetup) {
        if (!this.IsRunning) {
            this._runningTest.Reset();
            this._runningTest.StartNew(testSetup.WaferSetups,
                testSetup.SetTemperature,
                testSetup.SetCurrent);
            this._logger.LogInformation("TestTransport: Temp: {SetTemp} Current:{Current}",testSetup.SetTemperature,testSetup.SetCurrent.Name);
            var result=await this._testLogDataService.StartNew(this._runningTest);
            if (!result.IsError) {
                this._testSetupComplete = true;
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
    public async Task Start(bool success, string? message) {
        if (success) {
            this._running = true;
            this._paused = false;
            this._loggingEnabled = true;
            this._first = true;
            await this._hubContext.Clients.All.OnTestStarted($"Test Started Successfully, Message: {message ?? "No Message"}");
        }
        await this._hubContext.Clients.All.OnTestStartedFailed($"Test failed to start, " +
                                                                $"Message: {message ?? "Unknown Error"}");
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestStartAck });
    }


    
    public async Task StartFrom(ControllerSavedState savedState) {
        if(ObjectId.TryParse(savedState.TestId,out var id)) {
            var result = await this._testLogDataService.GetTestLogNoReadings(id);
            if (!result.IsError) {
                await StartFromControllerState(result.Value,savedState);
            } else {
                //TODO Handle this, temp start unknown and send warning
                await this.StartFromUnknown(savedState);
            }
        } else {
            //TODO Handle this, temp start unknown and send warning
            await this.StartFromUnknown(savedState);
        }
    }

    public async Task StopAndSaveState() {
        if (this._running) {
            var savedState = new ControllerSavedState(this._latestData);
            savedState.TestId = this._runningTest._id.ToString();
            //TODO: Save state to dababse and send reset signal to controller
        }
    }

    public Task UpdateSavedState(StationSerialData data) {
        var savedState = new ControllerSavedState(data);
        return this._testLogDataService.SetSavedState(this._stationId ?? "S01",savedState);
    }
    
    private async Task StartFromControllerState(BurnInTestLog log,ControllerSavedState savedState) {
        var setStateResult=await this._testLogDataService.SetSavedState(this._stationId ?? "S01",savedState);
        if (setStateResult.IsError) {
            this._logger.LogWarning("Failed to set saved state for station {StationId}",this._stationId ?? "S00");
            await this._hubContext.Clients.All.OnSetStationSavedStateFailed($"Failed to set saved state for station " +
                                                                            $"{this._stationId ?? "S00"}");
        }
        
        if (this._stationId != savedState.TestId) {
            this._logger.LogWarning("StationId({LogStationId}) from log({TestId}) " +
                                    "does not match systems stationId({StationId})"
                ,this._runningTest.StationId,savedState.TestId,this._stationId ?? "S01");
            await this._hubContext.Clients.All.OnSavedStateIdMatchStationId($"StationId({this._runningTest.StationId}) " +
                                                                            $"from log({this._runningTest._id}) " +
                                                                            $"does not match systems stationId({this._stationId ?? "S00"})");
        }
        
        this._runningTest = log;
        this._running = true;
        this._paused = false;
        this._loggingEnabled = true;
        this._first = false;
        await this._hubContext.Clients.All.OnTestStartedFrom(new LoadTestSetupTransport() {
            Success = true,
            Message = "Test loaded from saved state",
            WaferSetups = this._runningTest.TestSetup,
            SetTemperature = this._runningTest.SetTemperature,
            SetCurrent = this._runningTest.SetCurrent
        });
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
                WaferSetups = this._runningTest.TestSetup,
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        } else {
            this._loggingEnabled=false;
            await this._hubContext.Clients.All.OnTestStartedFromUnknown(new LoadTestSetupTransport() {
                Success = false,
                Message = "Saved state was not found and failed to create unknown test. " +
                          "Test will continue to run but no logs will be available.",
                WaferSetups = this._runningTest.TestSetup,
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        }
    }
    private async Task StartUnknown(StationCurrent current,int setTemp) {
        this.CreateUnknownTest(current,setTemp);
        var testResult=await this._testLogDataService.StartNewUnknown(this._runningTest,current);
        if (!testResult.IsError) {
            this._runningTest = testResult.Value;
            this._running = true;
            this._paused = false;
            this._loggingEnabled = true;
            this._first = true;
            await this._hubContext.Clients.All.OnTestStartedFromUnknown(new LoadTestSetupTransport() {
                Success = false,
                Message = "Warning: failed to load savedState. An Unknown test was started instead.",
                WaferSetups = this._runningTest.TestSetup,
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        } else {
            this._runningTest = testResult.Value;
            this._running = true;
            this._paused = false;
            this._loggingEnabled = false;
            this._first = false;
            await this._hubContext.Clients.All.OnTestStartedFromUnknown(new LoadTestSetupTransport() {
                Success = false,
                Message = "Error: Failed to load saved state and failed to create fallback unknown test." +
                          "test will continue to run but no logs will be available",
                WaferSetups = this._runningTest.TestSetup,
                SetTemperature = this._runningTest.SetTemperature,
                SetCurrent = this._runningTest.SetCurrent
            });
        }
    }
    public async Task StopTest() {
        if (this._running) {
            var result=await this._testLogDataService.SetCompleted(this._runningTest._id,
                this._stationId ?? "S01",DateTime.Now);
            if (!result.IsError) {
                await this._hubContext.Clients.All.OnTestCompleted("Test Completed");
            } else {
                await this._hubContext.Clients.All.OnTestCompleted("Error: Failed to mark test as completed");
            }
        } else {
            await this._hubContext.Clients.All.OnTestCompleted("Waring: Received stop command but no test is running");
        }
        await this._mediator.Send(new SendAckCommand() { AcknowledgeType = AcknowledgeType.TestCompleteAck });
    }
    
    private void CreateUnknownTest(StationCurrent current,int setTemp) {
        BurnInTestLog log=new BurnInTestLog { _id = ObjectId.GenerateNewId() };
        log.TestSetup.Add(new WaferSetup() {
            WaferId = "Unknown",
            Probe1 = StationProbe.Probe1,
            Probe2 = StationProbe.Probe2,
            StationPocket = StationPocket.LeftPocket
        });
        log.TestSetup.Add(new WaferSetup() {
            WaferId = "Unknown",
            Probe1 = StationProbe.Probe3,
            Probe2 = StationProbe.Probe4,
            StationPocket = StationPocket.MiddlePocket
        });
        log.TestSetup.Add(new WaferSetup() {
            WaferId = "Unknown",
            Probe1 = StationProbe.Probe5,
            Probe2 = StationProbe.Probe6,
            StationPocket = StationPocket.LeftPocket
        });
        log.SetCurrent = current;
        log.SetTemperature= setTemp;
        log.StationId = this._stationId ?? "S01";
        log.StartTime= DateTime.Now;
        log.Completed = false;
        log.ElapsedTime = 0;
        this._runningTest = log;
    }
    public async Task Log(StationSerialData data) {
        this._latestData = data;
        if (this._loggingEnabled) {
            if (this._first) {
                this._first = false;
                this._running = data.Running;
                this._paused = data.Paused;
                await this._testLogDataService.SetStart(this._runningTest._id,DateTime.Now,data);
                this._lastLog = DateTime.Now;
            } else {
                if ((DateTime.Now - this._lastLog).Seconds > this._interval.Seconds) {
                    this._lastLog = DateTime.Now;
                    await this._testLogDataService.InsertReading(this._runningTest._id,data);
                    await this.UpdateSavedState(data);
                } else {
                    if(this._paused!=data.Paused) {
                        this._paused = data.Paused;
                        await this._testLogDataService.InsertReading(this._runningTest._id,data);
                        await this.UpdateSavedState(data);
                    }
                }
            }
        }
    }
}
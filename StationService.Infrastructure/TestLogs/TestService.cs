using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.Wafers;
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
    private StationSerialData _latestData;
    private BurnInTestLog _runningTest=new BurnInTestLog();
    private IHubContext<StationHub, IStationHub> _hubContext;
    private IMediator _mediator;
    private string? _stationId;
    private DateTime _lastLog;
    private readonly TimeSpan _interval=new TimeSpan(0,0,60);
    private readonly ILogger<TestService> _logger;
    private List<StationSerialData> _readings=new List<StationSerialData>();

    private bool _running=false, _paused=false;
    private bool _loggingEnabled = false;
    private bool _testSetupComplete = false;
    private bool _first = false;

    public bool IsRunning => this._running;
    
    TestService(TestLogDataService testLogDataService,
        IHubContext<StationHub, IStationHub> hubContext,
        IMediator mediator,
        ILogger<TestService> logger) {
        this._logger = logger;
        this._mediator = mediator;
        this._testLogDataService = testLogDataService;
        this._hubContext = hubContext;
    }
    
    public Task Start(bool success, string? testId, string? message) {
        if (success) {
            this._running = true;
            this._paused = false;
            this._loggingEnabled = true;
            this._first = true;
            return this._hubContext.Clients.All.OnTestStarted($"Test Started Successfully, Message: {message ?? "No Message"}");
        }
        return this._hubContext.Clients.All.OnTestStartedFailed($"Test failed to start, " +
                                                                $"Message: {message ?? "Unknown Error"}");
    }

    public async Task StartFrom(string? message, string? testId,StationCurrent current,int setTemp) {
        if (!string.IsNullOrEmpty(testId)) {
            if (ObjectId.TryParse(testId, out var id)) {
                if (this._runningTest._id == id) {
                    this._running = true;
                    this._paused = false;
                    this._loggingEnabled = true;
                    this._first = false;
                    await this._hubContext.Clients.All.OnTestStartedFrom(new TestSetupTransport() {
                        Success = true,
                        Message ="Test loaded from saved state.  Continuing test",
                        WaferSetups = this._runningTest.TestSetup,
                        SetCurrent = this._runningTest.SetCurrent,
                        SetTemperature = this._runningTest.SetTemperature
                    });
                } else {
                    var result= await this._testLogDataService.TryContinueFrom(this._stationId ?? "S00",id);
                    if (!result.IsError) {
                        this._running = true;
                        this._paused = false;
                        this._loggingEnabled = true; 
                        this._first = false;
                        this._runningTest= result.Value;
                        await this._hubContext.Clients.All.OnTestStartedFrom(new TestSetupTransport() {
                            Success = true,
                            Message ="Test loaded from saved state.  Continuing test",
                            WaferSetups = this._runningTest.TestSetup,
                            SetCurrent = this._runningTest.SetCurrent,
                            SetTemperature = this._runningTest.SetTemperature
                        });
                    } else {
                        if (result.FirstError == Error.NotFound()) {
                        }else if (result.FirstError == Error.Conflict()) {
       
                        }else if(result.FirstError == Error.Failure()) {

                        }
                        this.CreateUnknownTest(current, setTemp);
                        this._loggingEnabled = false;
                        this._running = true;
                        this._paused = false;
                        this._first = false;
                        await this._hubContext.Clients.All.OnTestStartedFrom(new TestSetupTransport() {
                            Success = true,
                            Message ="Error: Could not find logged test. " +
                                     "\n Test will continue running without logging enabled",
                            WaferSetups = this._runningTest.TestSetup,
                            SetCurrent = this._runningTest.SetCurrent,
                            SetTemperature = this._runningTest.SetTemperature
                        });
                    }
                }
            } else {
                var result=await this._testLogDataService.TryContinueFrom(this._stationId ?? "S00");
                if(result.FirstError==Error.NotFound()) {
                    await this._hubContext.Clients.All.OnTestStartedFailed("Failed to parse test id and no running test found");
                } else if (result.FirstError == Error.Conflict()) {
                    await this._hubContext.Clients.All.OnTestStartedFailed("Failed to parse test id and running test id does not match");
                } else if (result.FirstError == Error.Unexpected()) {
                    await this._hubContext.Clients.All.OnTestStartedFailed("Failed to parse test id and failed to parse running test id");
                }
            }
        } else {
            var result=await this._testLogDataService.TryContinueFrom(this._stationId ?? "S00");
            if(result.FirstError==Error.NotFound()) {
                await this._hubContext.Clients.All.OnTestStartedFailed("No test id provided and no running test found");
            } else if (result.FirstError == Error.Conflict()) {
                await this._hubContext.Clients.All.OnTestStartedFailed("No test id provided and running test id does not match");
            } else if (result.FirstError == Error.Unexpected()) {
                await this._hubContext.Clients.All.OnTestStartedFailed("No test id provided and failed to parse running test id");
            }
        }
        await this._mediator.Send(new SendAckCommand(){AcknowledgeType = AcknowledgeType.TestStartAck});
    }

    public async Task SetupTest(List<WaferSetup> setup,StationCurrent current,int setTemp) {
        if (!this.IsRunning) {
            this._runningTest.Reset();
            this._runningTest.StartNew(setup,setTemp,current);
            var result=await this._testLogDataService.StartNew(this._runningTest);
            if (!result.IsError) {
                this._testSetupComplete = true;
                await this._hubContext.Clients.All.OnTestSetup(true, "Test Setup Complete, start test when ready");
            } else {
                this._testSetupComplete = false;
                await this._hubContext.Clients.All.OnTestSetup(false,result.FirstError.Description);
                this._logger.LogError("Failed to start new test.  Internal Error: " + result.FirstError);
            }
        } else {
            await this._hubContext.Clients.All.OnTestSetup(false,"Test is already running, " +
                                                                 "please reset controller or wait for current " +
                                                                 "test to complete before starting new test");
        }
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
        log.StationId = this._stationId ?? "S00";
        log.StartTime= DateTime.Now;
        log.Completed = false;
        log.ElapsedTime = 0;
        this._runningTest = log;
    }

    public Task Log(StationSerialData data) {
        if (this._loggingEnabled) {
            
        }
        return Task.CompletedTask;
    }
    
}
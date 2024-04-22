using BurnInControl.Application.BurnInTest;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.HubDefinitions.Hubs;
using ErrorOr;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StationService.Infrastructure.Hub;
namespace StationService.Infrastructure.TestLogs;

public class BurnInTestService:IBurnInTestService{
    private readonly TestLogDataService _testLogDataService;
    private StationSerialData _latestData;
    private BurnInTestLog _runningTest=new BurnInTestLog();
    private IHubContext<StationHub, IStationHub> _hubContext;
    private bool _controllerStartedTest=false;
    private bool _testRunning = false;
    private bool _testPaused = false;
    private bool _disableLogging = false;
    private bool _testSetupComplete = false;
    private string? _stationId;
    private DateTime _lastLog;
    private readonly TimeSpan _interval=new TimeSpan(0,0,60);
    private readonly ILogger<BurnInTestService> _logger;
    private List<StationSerialData> _readings=new List<StationSerialData>();
    
    public bool IsRunning => this._testRunning || this._testPaused;

    public BurnInTestService(TestLogDataService testLogDataService,
        ILogger<BurnInTestService> logger,
        IOptions<StationSettings> options,
        IHubContext<StationHub, IStationHub> hubContext) {
        this._logger = logger;
        this._testLogDataService = testLogDataService;
        this._stationId = options.Value.StationId;
        this._hubContext = hubContext;
    }
    
    public async Task SetupTest(List<WaferSetup> setup,StationCurrent current,int setTemp) {
        if (!this.IsRunning) {
            this._controllerStartedTest = false;
            this._runningTest.StartNew(setup,setTemp,current);
            var result=await this._testLogDataService.StartNew(this._runningTest);
            if (!result.IsError) {
                await this._hubContext.Clients.All.OnTestSetup(true, "Test Setup Complete, start test when ready");
            } else {
                this._testSetupComplete = false;
                this._testRunning = false;
                this._testPaused = false;
                this._disableLogging = false;
                await this._hubContext.Clients.All.OnTestSetup(false,result.FirstError.Description);
                this._logger.LogError("Failed to start new test.  Internal Error: " + result.FirstError);
            }
        } else {
            await this._hubContext.Clients.All.OnTestSetup(false,"Test is already running, " +
                                                                 "please reset controller or wait for current " +
                                                                 "test to complete before starting new test");
        }
    }

    public async Task StartTest() {
        if (this._testSetupComplete) {
            this._disableLogging = false;
            await this._testLogDataService.SetStart(this._runningTest._id,DateTime.Now,this._latestData);
        }else {
            if (!string.IsNullOrEmpty(this._stationId)) {
                //TODO: Log to station with unknown test setup
                await this._hubContext.Clients.All.OnTestStarted("Unexpected Error: " +
                                                                 "Test Setup not complete but received start from controller." +
                                                                 "Log will be saved as unknown test setup.  Please contact the administrator.");
                this._logger.LogError("Unexpected Error: Test Setup not complete but received start from controller");
            } else {
                //TODO: Log to unknown station with unknown test setup
                await this._hubContext.Clients.All.OnTestStarted("Unexpected Error: " +
                                                                 "Test Setup not complete but received start from controller." +
                                                                 "Log will be saved as unknown test setup.  Please contact the administrator.");
                this._logger.LogError("Unexpected Error: Test Setup not complete but received start from controller");
            }

        }
    }

    public async Task StartTestFrom() {
        if (!string.IsNullOrEmpty(this._stationId)) {

        } else {
            //this._hubContext.Clients.All.OnTestStarted
            this._logger.LogCritical("StationId is null or empty, cannot continue test.  " +
                                     "Controller will continue running.  " +
                                     "Please contact the administrator");
        }
        
    }

    public Task<ErrorOr<Success>> SetupTest(List<WaferSetup> setup) {
        throw new NotImplementedException();
    }

    public void StartTestLogging() {
        //this._runningTest.SetStart(DateTime.Now,data);

    }

    ErrorOr<Success> IBurnInTestService.Log(StationSerialData data) {
        throw new NotImplementedException();
    }

    public void CompleteTest() {
        this._controllerStartedTest = false;
    }
    
    public async Task Log(StationSerialData data) {
        if (!this.IsRunning && data.Running) {
            this.LogStart(data);
        }
        if (this.IsRunning && !data.Running) {
            this.LogFinished(data);
        }
        if(this.IsRunning && data.Running){
            this._latestData = data;
            this._testRunning = data.Running;
            this._testPaused = data.Paused;
            this._runningTest.AddReading(data);
            if (!this._testPaused && !this._disableLogging) {
                var now = DateTime.Now;
                if ((now - this._lastLog >= this._interval)) {
                    this._lastLog = now;
                    await this._testLogDataService.InsertReading(this._runningTest._id,data);
                }
            }
            
        }
        
        this._latestData = data;
    }

    private void LogStart(StationSerialData data) {
        this._latestData = data;
        if (this._controllerStartedTest) {
            this._runningTest.SetStart(DateTime.Now,data);
            this._testRunning = this._latestData.Running;
            this._testPaused = this._latestData.Paused;
            this._disableLogging = false;
            //TODO: Log to database
            
        } else {
            //Try to find running test and continue logging
            
        }
    }

    private ErrorOr<Success> ContinueTest(StationSerialData data) {
        bool testFound = false;
        //TODO: Search for test to continue
        //for testing pretend test was found
        testFound = true;
        if (testFound) {
            this._controllerStartedTest = true;
            this._testRunning = data.Running;
            this._testPaused = data.Paused;
            this._disableLogging = false;
            return Result.Success;
        } else {
            this._controllerStartedTest = true;
            this._testRunning = true;
            this._testPaused = false;
            this._disableLogging = true;
            return Error.NotFound(description:"Failed to find running test," +
                                              "test will continue running without logging");
        }
    }
    private ErrorOr<Success> LogFinished(StationSerialData data) {
        this._testRunning = false;
        this._testRunning = false;
        this._controllerStartedTest = false;
        this._disableLogging = false;
        this._latestData = data;
        this._runningTest.SetCompleted(DateTime.Now);
        //TODO: Log to database
        return Result.Success;
    }
}
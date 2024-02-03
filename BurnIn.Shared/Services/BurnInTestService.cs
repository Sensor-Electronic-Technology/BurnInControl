using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
namespace BurnIn.Shared.Services;
public class BurnInTestService {
    public event EventHandler TestStartedHandler;
    public event EventHandler TestCompleteHandler;
    
    private StationSerialData _latestData;
    private BurnInTestLog _runningTest=new BurnInTestLog();
    private bool _testSetupComplete=false;
    private bool _testRunning = false;
    private bool _testPaused = false;
    private bool _disableLogging = false;
    private DateTime _lastLog;
    private readonly TimeSpan _interval=new TimeSpan(0,0,60);
    private readonly ILogger<BurnInTestService> _logger;

    public bool IsRunning => this._testRunning || this._testPaused;

    public BurnInTestService(ILogger<BurnInTestService> logger,IMongoClient client) {
        this._logger = logger;
        //TODO: setup database connection
        //var database=client.GetDatabase("database")
        //this._collection=database.getCollection
    }

    public Result SetupTest(List<WaferSetup> setup) {
        if (!this.IsRunning) {
            this._testSetupComplete = false;
            this._runningTest.StartNew(setup);
            return ResultFactory.Success();
        }
        return ResultFactory.Error("Cannot create a new test while a test is running");
    }

    public void SetSetupComplete() {
        this._testSetupComplete=true;
    }
    
    public Result Log(StationSerialData data) {
        if (!this.IsRunning && data.Running) {
            //start
            return this.LogStart(data);
        }
        if (this.IsRunning && !data.Running) {
            //stop
            return this.LogFinished(data);
        }
        if(this.IsRunning && data.Running){
            //log
            this._latestData = data;
            this._testRunning = data.Running;
            this._testPaused = data.Paused;
            this._runningTest.AddReading(data);
            if (!this._testPaused && !this._disableLogging) {
                var now = DateTime.Now;
                if ((now - this._lastLog >= this._interval)) {
                    this._lastLog = now;
                    //TODO: Log to database
                }
            }
            return ResultFactory.Success();
        }
        
        this._latestData = data;
        return ResultFactory.Success();
    }

    private Result LogStart(StationSerialData data) {
        this._latestData = data;
        if (this._testSetupComplete) {
            this._runningTest.SetStart(DateTime.Now,data);
            this._testRunning = this._latestData.Running;
            this._testPaused = this._latestData.Paused;
            this._disableLogging = false;
            //TODO: Log to database
            return ResultFactory.Success("Test Started");
        } else {
            //Try to find running test and continue logging
            return this.ContinueTest(data);
        }
    }
    /***
     * If test not found continue test without logging
     */
    private Result ContinueTest(StationSerialData data) {
        bool testFound = false;
        //TODO: Search for test to continue
        //for testing pretend test was found
        testFound = true;
        if (testFound) {
            this._testSetupComplete = true;
            this._testRunning = data.Running;
            this._testPaused = data.Paused;
            this._disableLogging = false;
            return ResultFactory.Success("Continuing test ABDC");
        } else {
            this._testSetupComplete = true;
            this._testRunning = true;
            this._testPaused = false;
            this._disableLogging = true;
            return ResultFactory.Error("Failed to find running test," +
                                   "test will continue running without logging");
        }
    }
    private Result LogFinished(StationSerialData data) {
        this._testRunning = false;
        this._testRunning = false;
        this._testSetupComplete = false;
        this._disableLogging = false;
        this._latestData = data;
        this._runningTest.SetCompleted(DateTime.Now);
        //TODO: Log to database
        return ResultFactory.Success("Test Completed");
    }
}
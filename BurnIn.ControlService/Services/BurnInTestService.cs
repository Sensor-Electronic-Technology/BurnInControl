using BurnIn.Shared;
using BurnIn.Shared.Models;
using MongoDB.Driver;
namespace BurnIn.ControlService.Services;

public class StationStatus {
    public bool Running { get; set; }
    public bool Paused { get; set; }
}

public class BurnInTestService {
    private StationSerialData _latestData;
    private bool _testSetupComplete=false;
    private bool _testRunning = false;
    private bool _testPaused = false;
    private bool _disableLogging = false;
    private DateTime _lastLog;
    private readonly TimeSpan _interval=new TimeSpan(0,0,60);
    private readonly ILogger<BurnInTestService> _logger;
    //IMongoCollection<>

    public bool IsRunning => this._testRunning || this._testPaused;

    public BurnInTestService(ILogger<BurnInTestService> logger,IMongoClient client) {
        this._logger = logger;
        //TODO: setup database connection
        //var database=client.GetDatabase("database")
        //this._collection=database.getCollection
    }

    public void SetupTest(/*Wafers,pads,etc*/) {
        this._testSetupComplete = true;
        //TODO setup test
        //TODO log
    }
    
    public Result Log(StationSerialData data) {
        if (!this.IsRunning && data.Running) {
            //start
            return this.LogStart(data);
        }else if (this.IsRunning && !data.Running) {
            //stop
            return this.LogFinished(data);
        } else if(this.IsRunning && data.Running){
            //log
            this._latestData = data;
            this._testRunning = data.Running;
            this._testPaused = data.Paused;
            if (!this._testPaused && !this._disableLogging) {
                var now = DateTime.Now;
                if ((now - this._lastLog >= this._interval)) {
                    this._lastLog = now;
                    //TODO: Log to database
                }
            }
            return new SuccessResult();
        } else {
            this._latestData = data;
            return new SuccessResult();
        }
    }

    private Result LogStart(StationSerialData data) {
        this._latestData = data;
        if (this._testSetupComplete) {
            this._testRunning = this._latestData.Running;
            this._testPaused = this._latestData.Paused;
            this._disableLogging = false;
            //TODO: Log to database
            return new SuccessResult("Test Started");
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
            return new SuccessResult("Continuing test ABDC");
        } else {
            this._testSetupComplete = true;
            this._testRunning = true;
            this._testPaused = false;
            this._disableLogging = true;
            return new ErrorResult("Failed to find running test," +
                                   "test will continue running without logging");
        }
    }
    private Result LogFinished(StationSerialData data) {
        this._testRunning = false;
        this._testRunning = false;
        this._testSetupComplete = false;
        this._disableLogging = false;
        this._latestData = data;
        //TODO: Log to database
        return new SuccessResult("Test Completed");
    }
}
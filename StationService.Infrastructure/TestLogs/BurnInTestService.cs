﻿using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Shared.ComDefinitions;
using ErrorOr;
using Microsoft.Extensions.Logging;
namespace StationService.Infrastructure.TestLogs;

public class BurnInTestService {
    private readonly TestLogDataService _testLogDataService;
    private StationSerialData _latestData;
    private BurnInTestLog _runningTest=new BurnInTestLog();
    private bool _controllerStartedTest=false;
    private bool _testRunning = false;
    private bool _testPaused = false;
    private bool _disableLogging = false;
    private DateTime _lastLog;
    private readonly TimeSpan _interval=new TimeSpan(0,0,60);
    private readonly ILogger<BurnInTestService> _logger;

    public bool IsRunning => this._testRunning || this._testPaused;

    public BurnInTestService(TestLogDataService testLogDataService,ILogger<BurnInTestService> logger) {
        this._logger = logger;
        this._testLogDataService = testLogDataService;
    }
    
    public Task<ErrorOr<Success>> SetupTest(List<WaferSetup> setup) {
        if (!this.IsRunning) {
            this._controllerStartedTest = false;
            this._runningTest.StartNew(setup);
            return Task.FromResult<ErrorOr<Success>>(Result.Success);
        }
        return Task.FromResult<ErrorOr<Success>>(Error.Forbidden(description:"Cannot create a new test while a test is running"));
    }

    public void StartTestLogging() {
        this._controllerStartedTest=true;
    }
    
    public ErrorOr<Success> Log(StationSerialData data) {
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
            return Result.Success;
        }
        
        this._latestData = data;
        return Result.Success;
    }

    private ErrorOr<Success> LogStart(StationSerialData data) {
        this._latestData = data;
        if (this._controllerStartedTest) {
            this._runningTest.SetStart(DateTime.Now,data);
            
            this._testRunning = this._latestData.Running;
            this._testPaused = this._latestData.Paused;
            this._disableLogging = false;
            //TODO: Log to database
            return Result.Success;
        } else {
            //Try to find running test and continue logging
            return this.ContinueTest(data);
        }
    }
    /***
     * If test not found continue test without logging
     */
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
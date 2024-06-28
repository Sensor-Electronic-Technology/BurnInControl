using BurnInControl.Data.BurnInTests;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;
using MongoDB.Bson;

namespace BurnInControl.Application.BurnInTest.Interfaces;

public interface ITestService {
    public bool IsRunning { get; }
    public long RemainingTimeSecs();
    public Task Start(bool success,string? message);
    public Task StartFrom(ControllerSavedState savedState);
    //public Task StartFrom(string? message, string? testId,StationCurrent current, int setTemp);
    public Task SetupTest(TestSetupTransport testSetupTransport);
    public Task CompleteTest();
    public Task StopAndSaveState();
    public Task SendRunningTest();
    public Task LoadState(ObjectId savedStateId);
    public Task Log(StationSerialData data);
    public Task Stop();
}
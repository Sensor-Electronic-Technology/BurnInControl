using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;
using MongoDB.Bson;

namespace BurnInControl.Application.BurnInTest.Interfaces;

public interface ITestService {
    public bool IsRunning { get; }
    public Task Start(bool success,string? message);
    public Task StartFrom(ControllerSavedState savedState);
    //public Task StartFrom(string? message, string? testId,StationCurrent current, int setTemp);
    public Task SetupTest(TestSetupTransport testSetupTransport);
    public Task CompleteTest();
    public Task StopAndSaveState();
    public Task LoadState(ObjectId logId);
    public Task Log(StationSerialData data);
    public Task Stop();
}
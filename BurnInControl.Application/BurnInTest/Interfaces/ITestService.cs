using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;

namespace BurnInControl.Application.BurnInTest.Interfaces;

public interface ITestService {
    public bool IsRunning { get; }
    public Task Start(bool success,string? message);
    public Task StartFrom(string? message, string? testId,StationCurrent current, int setTemp);
    public Task SetupTest(TestSetupTransport testSetupTransport);
    public Task StopTest();
    public Task Log(StationSerialData data);
}
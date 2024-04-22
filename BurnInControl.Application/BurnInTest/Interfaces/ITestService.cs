using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Shared.ComDefinitions;

namespace BurnInControl.Application.BurnInTest.Interfaces;

public interface ITestService {
    public Task Start(bool success,string? testId,string? message);
    public Task StartFrom(string? message, string? testId,StationCurrent current, int setTemp);
    public Task SetupTest(List<WaferSetup> setup, StationCurrent current, int setTemp);
    public Task Log(StationSerialData data);
}
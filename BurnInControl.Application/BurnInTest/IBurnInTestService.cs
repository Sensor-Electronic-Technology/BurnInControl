using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Shared.ComDefinitions;
using ErrorOr;
namespace BurnInControl.Application.BurnInTest;

public interface IBurnInTestService
{
    bool IsRunning { get; }
    Task<ErrorOr<Success>> SetupTest(List<WaferSetup> setup);
    void StartTestLogging();
    ErrorOr<Success> Log(StationSerialData data);
}
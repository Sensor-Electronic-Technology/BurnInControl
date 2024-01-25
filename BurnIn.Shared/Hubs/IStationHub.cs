using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
namespace BurnIn.Shared.Hubs;

public interface IStationHub {
    Task OnUsbConnect(bool connected);
    Task OnUsbDisconnect(bool disconnected);
    Task OnExecuteCommand(bool executed);
    Task OnSerialCom(StationSerialData reading);
    Task OnIdChanged(string Id);
    Task OnSerialComMessage(string message);
    Task OnSettingsUploaded(bool uploaded);
    
    //Test Related
    Task OnTestStarted(string message);
    Task OnTestStartedFailed(string message);
    Task OnTestPaused(string message);
    Task OnTestPausedFailed(string message);
    Task OnTestContinued(string message);
    Task OnTestContinuedFailed(string message);
    Task OnTestCompleted(string message);
    Task OnTestCompletedFailed(string message);
}
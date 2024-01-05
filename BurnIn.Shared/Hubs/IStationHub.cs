using BurnIn.Shared.Models.BurnInStationData;
namespace BurnIn.Shared.Hubs;

public interface IStationHub {
    Task OnUsbConnect(bool connected);
    Task OnUsbDisconnect(bool disconnected);
    Task OnExecuteCommand(bool executed);
    Task OnSerialCom(RawReading reading);
    Task OnSerialComMessage(string message);
    Task OnSettingsUploaded(bool uploaded);
}
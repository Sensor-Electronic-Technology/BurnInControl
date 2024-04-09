using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
namespace BurnInControl.Shared.Hubs;

public interface IStationHub {

#region StationNotifications
    Task OnSerialCom(StationSerialData serialData);

    Task OnSerialComError(StationMsgPrefix prefix,string message);
    Task OnSerialComMessage(string message);
    Task OnSerialNotifyMessage(string message);
    Task OnSerialErrorMessage(string message);
    Task OnSerialInitMessage(string message);

#endregion  

#region TestStatus
    Task OnTestStarted(string message);
    Task OnTestStartedFailed(string message);
    Task OnTestCompleted(string message);
#endregion

#region UsbStatus
    Task OnUsbConnectFailed(string message);
    Task OnUsbDisconnect(string message);
    Task OnUsbConnect(string message);
    Task OnUsbDisconnectFailed(string message);
#endregion





}
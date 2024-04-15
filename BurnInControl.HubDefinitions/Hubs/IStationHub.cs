using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
namespace BurnInControl.HubDefinitions.Hubs;

public interface IStationHub {

#region StationNotifications
    Task OnSerialCom(StationSerialData serialData);
    Task OnSerialComError(StationMsgPrefix prefix,string message);
    Task OnSerialComMessage(string message);
    Task OnSerialNotifyMessage(string message);
    Task OnSerialErrorMessage(string message);
    Task OnSerialInitMessage(string message);

#endregion  

#region BurnInTest
    Task OnTestStarted(string message);
    Task OnTestStartedFrom();
    Task OnTestStartedFromUnknown(TestSetupTransport testSetup);
    Task OnTestStartedFailed(string message);
    Task OnTestCompleted(string message);
    Task OnTestSetup(bool success, string message);
    
#endregion

#region ConnectionStatus
    Task OnUsbConnectFailed(string message);
    Task OnUsbDisconnect(string message);
    Task OnUsbConnect(string message);
    Task OnUsbDisconnectFailed(string message);
    Task OnStationConnection(bool usbStatus);
    #endregion





}
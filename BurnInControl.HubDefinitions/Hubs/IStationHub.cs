using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.Shared.FirmwareData;

namespace BurnInControl.HubDefinitions.Hubs;

public interface IStationHub {

#region StationNotifications
    Task OnStationData(StationSerialData serialData);
    Task OnTuningData(TuningSerialData tuningData);
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

#region FirmwareUpdateNotifications
    Task OnFirmwareUpdateCheck(UpdateCheckStatus checkStatus);
    Task OnFirmwareUpdateCheckFailed(string message);
    Task OnFirmwareUpdateStarted();
    Task OnFirmwareUpdateFailed(string errorMessage);
    Task OnFirmwareUpdateCompleted();
    Task OnFirmwareDownloaded(bool success,string message);
    Task OnFirmwareUpdated(string version,string avrOutput);
#endregion

#region ConnectionStatus
    Task OnUsbConnectFailed(string message);
    Task OnUsbDisconnect(string message);
    Task OnUsbConnect(string message);
    Task OnUsbDisconnectFailed(string message);
    Task OnStationConnection(bool usbStatus);
    #endregion





}
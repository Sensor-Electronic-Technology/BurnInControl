using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.HubDefinitions.HubTransports;
using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.Shared.FirmwareData;

namespace BurnInControl.HubDefinitions.Hubs;


public interface IStationHub {

#region StationNotifications
    Task OnStationData(StationSerialData serialData);
    Task OnTuningData(TuningSerialData tuningData);
    Task OnSerialComError(StationMsgPrefix prefix,string message);
    Task OnSerialComMessage(int type,string message);
    Task OnProbeTestDone();
    Task OnConfigSaveStatus(string type,bool success, string message);
    Task OnRequestConfigHandler(bool success,int configType,string jsonConfig);
    Task OnSwTuneNotify(int mode);
    Task OnNotifyHeaterTuningStatus(HeaterTuneResult result);
    Task OnNotifyHeaterTuneComplete(List<HeaterTuneResult> results);
    Task OnTuningResultsSavedDatabase(bool success,string message);
#endregion  

#region BurnInTest
    Task OnTestStarted(string message);
    Task OnTestStartedFrom(LoadTestSetupTransport testSetupTransport);
    Task OnTestStartedFromUnknown(LoadTestSetupTransport testSetupTransport);
    Task OnTestStartedFailed(string message);
    Task OnTestCompleted(string message);
    Task OnTestSetup(bool success, string message);
    Task OnLoadFromSavedState(TestSetupTransport testSetupTransport);
    Task OnRequestRunningTest(LoadTestSetupTransport testSetupTransport);
    Task OnLoadFromSavedStateError(string message);
    Task OnStopAndSaved(bool success, string message);
    
#endregion

#region UpdateNotifications
    Task OnUpdateStart(string message);
    Task OnUpdateComplete(bool success,string message);
#endregion

#region ConnectionStatus
    Task OnUsbConnectFailed(string message);
    Task OnUsbDisconnect(string message);
    Task OnUsbConnect(string message);
    Task OnUsbDisconnectFailed(string message);
    Task OnStationConnection(bool usbStatus);
    #endregion





}
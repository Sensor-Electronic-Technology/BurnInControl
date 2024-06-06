namespace BurnInControl.HubDefinitions.Hubs;

public static class HubConstants {
    //public static string HubAddress => "http://192.168.68.112:3000/hubs/station";
    //public static string HubAddress => "http://172.20.1.15:3000/hubs/station";
    //public static string HubAddress => "http://localhost:5000/hubs/station";
    /*public static string HubAddress=> "http://station.service:5000/hubs/station";*/
    
    public static string HubAddress => "http://192.168.68.111:5000/hubs/station";

    public static class Events {
        public static string OnStationData => "OnStationData";
        public static string OnTuningData => "OnTuningData";
        public static string OnSerialComMessage => "OnSerialComMessage";
        public static string OnUsbConnect => "OnUsbConnect";
        public static string OnUsbDisconnect => "OnUsbDisconnect";
        public static string OnUsbConnectFailed => "OnUsbConnectFailed";
        public static string OnUsbDisconnectFailed => "OnUsbDisconnectFailed";
        public static string OnExecuteCommand => "OnExecuteCommand";
        public static string OnConfigSaveStatus => "OnConfigSaveStatus";
        public static string OnTestStarted => "OnTestStarted";
        public static string OnTestStartedFrom => "OnTestStartedFrom";
        public static string OnTestStartedFromUnknown => "OnTestStartedFromUnknown";
        public static string OnTestStartedFailed => "OnTestStartedFailed";
        public static string OnTestCompleted => "OnTestCompleted";
        public static string OnTestSetup => "OnTestSetup";
        public static string OnUpdateChecked => "OnUpdateChecked";
        public static string OnFirmwareUpdated => "OnFirmwareUpdated";
        public static string OnStationConnection => "OnStationConnection";
        public static string OnRequestConfigHandler => "OnRequestConfigHandler";
        public static string OnProbeTestDone => "OnProbeTestDone";
        public static string OnRequestRunningTest => "OnRequestRunningTest";
        public static string OnTuningResultsSavedDatabase => "OnTuningResultsSavedDatabase";
        public static string OnSwTuneNotify => "OnSwTuneNotify";
        public static string OnNotifyHeaterTuningStatus => "OnNotifyHeaterTuningStatus";
        public static string OnNotifyHeaterTuneComplete => "OnNotifyHeaterTuneComplete";
    }

    public static class Methods {
        public static string ConnectUsb => "ConnectUsb";
        public static string DisconnectUsb => "DisconnectUsb";
        public static string SetupTest=> "SetupTest";
        public static string SendHeaterControlConfig => "SendHeaterControlConfig";
        public static string SendProbeControlConfig => "SendProbeControlConfig";
        public static string SendStationConfig => "SendStationConfig";
        public static string SendCommand => "SendCommand";
        public static string RequestConfig=> "RequestConfig";
        public static string SaveTuningResults=> "SaveTuningResults";
        public static string SendTuningWindowSize=> "SendTuningWindowSize";
    }

}
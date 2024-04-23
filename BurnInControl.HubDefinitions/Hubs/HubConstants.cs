namespace BurnInControl.HubDefinitions.Hubs;

public static class HubConstants {
    //public static string HubAddress => "http://192.168.68.112:3000/hubs/station";
    //public static string HubAddress => "http://172.20.1.15:3000/hubs/station";
    //public static string HubAddress => "http://localhost:5000/hubs/station";
    /*public static string HubAddress=> "http://station.service:5000/hubs/station";*/
    
    public static string HubAddress => "http://192.168.68.112:5000/hubs/station";

    public static class Events {
        public static string OnStationData => "OnStationData";
        public static string OnTuningData => "OnTuningData";
        public static string OnSerialComMessage => "OnSerialComMessage";
        public static string OnUsbConnect => "OnUsbConnect";
        public static string OnUsbDisconnect => "OnUsbDisconnect";
        public static string OnUsbConnectFailed => "OnUsbConnectFailed";
        public static string OnUsbDisconnectFailed => "OnUsbDisconnectFailed";
        public static string OnExecuteCommand => "OnExecuteCommand";
        public static string OnIdChanged => "OnIdChanged";

        public static string OnTestStatus => "OnTestStatus";
        public static string OnTestPaused => "OnTestPaused";
        public static string OnTestContinued => "OnTestContinued";
        public static string OnTestCompleted => "OnTestCompleted";
        
        public static string OnTestStartedFailed => "OnTestStartedFailed";
        public static string OnTestPausedFailed => "OnTestPausedFailed";
        public static string OnTestContinuedFailed => "OnTestContinuedFailed";
        public static string OnTestCompletedFailed => "OnTestCompletedFailed";
        public static string OnReceiveFirmwareUploadText => "OnReceiveFirmwareUploadText";
        public static string SetupTest => "SetupTest";

        public static string OnUpdateChecked => "OnUpdateChecked";
        public static string OnFirmwareUpdated => "OnFirmwareUpdated";
        
        public static string OnStationConnection => "OnStationConnection";

    }

    public static class Methods {
        public static string ConnectUsb => "ConnectUsb";
        public static string DisconnectUsb => "DisconnectUsb";
        public static string SetupTest=> "SetupTest";
        public static string SendHeaterConfig => "SendHeaterConfig";
        public static string SendProbeConfig => "SendProbeConfig";
        public static string SendStationConfig => "SendStationConfig";
        public static string SendCommand => "SendCommand";
        public static string SendId => "SendId";
        public static string RequestId => "RequestId";
        public static string CheckForUpdate => "CheckForUpdate";
        public static string UpdateFirmware => "UpdateFirmware";
        public static string SendFirmwareVersion => "SendFirmwareVersion";
        public static string OnTestSetupSucceeded => "OnTestSetupSucceeded";
        public static string OnTestSetupFailed => "OnTestSetupFailed";
        
    }

}
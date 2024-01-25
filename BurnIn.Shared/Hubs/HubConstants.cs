using Ardalis.SmartEnum;
namespace BurnIn.Shared.Hubs;

public static class HubConstants {
    public static string HubAddress => "http://192.168.68.112:3000/hubs/station";

    public static class Events {
        public static string OnSerialCom => "OnSerialCom";
        public static string OnSerialComMessage => "OnSerialComMessage";
        public static string OnUsbConnect => "OnUsbConnect";
        public static string OnUsbDisconnect => "OnUsbDisconnect";
        public static string OnExecuteCommand => "OnExecuteCommand";
        public static string OnIdChanged => "OnIdChanged";

        public static string OnTestStarted => "OnTestStarted";
        public static string OnTestPaused => "OnTestPaused";
        public static string OnTestContinued => "OnTestContinued";
        public static string OnTestCompleted => "OnTestCompleted";
        
        public static string OnTestStartedFailed => "OnTestStartedFailed";
        public static string OnTestPausedFailed => "OnTestPausedFailed";
        public static string OnTestContinuedFailed => "OnTestContinuedFailed";
        public static string OnTestCompletedFailed => "OnTestCompletedFailed";
        
    }

    public static class Methods {
        public static string ConnectUsb => "ConnectUsb";
        public static string Disconnect => "ConnectUsb";
        public static string Send => "Send";
        public static string SendHeaterConfig => "SendHeaterConfig";
        public static string SendProbeConfig => "SendProbeConfig";
        public static string SendStationConfig => "SendStationConfig";
        public static string SendCommand => "SendCommand";
        public static string SendId => "SendId";
        public static string RequestId => "RequestId";

    }

}
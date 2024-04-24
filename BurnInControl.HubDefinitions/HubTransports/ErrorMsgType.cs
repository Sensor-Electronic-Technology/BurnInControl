using Ardalis.SmartEnum;

namespace BurnInControl.HubDefinitions.HubTransports;

public class ErrorMessageType: SmartEnum<ErrorMessageType, string> {
    public static readonly ErrorMessageType ControllerError = new ErrorMessageType(nameof(ControllerError),"Controller");
    public static readonly ErrorMessageType TestServiceError = new ErrorMessageType(nameof(TestServiceError),"TestService");
    public static readonly ErrorMessageType StationMessageHandlerError = new ErrorMessageType(nameof(StationMessageHandlerError),"StationMessageHandler");
    public static readonly ErrorMessageType StationControllerError= new ErrorMessageType(nameof(StationControllerError),"StationController");
    public static readonly ErrorMessageType UsbControllerError = new ErrorMessageType(nameof(UsbControllerError),"UsbController");
    private ErrorMessageType(string name,string value) : base(name,value) { }
}
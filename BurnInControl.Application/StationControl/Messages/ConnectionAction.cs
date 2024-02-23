namespace BurnInControl.Application.StationControl.Messages;

public enum ConnectAction {
    Connect,
    Disconnect
}

public class ConnectionAction {
    public ConnectAction Action { get; set; }
}
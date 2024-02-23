using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
namespace BurnInControl.Messages;

public class SendUsbCommand {
    public StationMsgPrefix MessagePrefix { get; set; }
    public IPacket Data { get; set; }
}

public class ConnectUsbCommand { }

public class DisconnectUsbCommand { }
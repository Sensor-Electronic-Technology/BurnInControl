using BurnIn.Data.ComDefinitions;
using BurnIn.Data.ComDefinitions.Station;
namespace BurnInControl.Messages;

public class SendUsbCommand {
    public StationMsgPrefix MessagePrefix { get; set; }
    public IPacket Data { get; set; }
}

public class ConnectUsbCommand { }

public class DisconnectUsbCommand { }
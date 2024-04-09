using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Shared.ComDefinitions.Packets;

/*public class StationMessage:IPacket {
    public string Message { get; set; }
    public StationMessage() {
        
    }
}*/

public enum StationMessageType {
    GENERAL,
    INIT,
    NOTIFY,
    ERROR,
}
public class StationMessagePacket : IPacket {
    public StationMessageType MessageType { get; set; }
    public string Message { get; set; }
    
    public StationMessagePacket() {
        
    }
    
}
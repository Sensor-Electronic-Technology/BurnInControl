using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Shared.ComDefinitions.Packets;

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
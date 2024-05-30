using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Shared.ComDefinitions.Packets;
public enum StationMessageType:int {
    GENERAL=0,
    INIT=1,
    NOTIFY=2,
    ERROR=3,
}
public class StationMessagePacket : IPacket {
    public StationMessageType MessageType { get; set; }
    public string Message { get; set; }
    
    public StationMessagePacket() {
        
    }
    
}
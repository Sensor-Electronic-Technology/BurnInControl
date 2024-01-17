namespace BurnIn.Shared.Models;


public class MessagePacket {
    public string Prefix { get; set; }
    public object Packet { get; set; }
}
using BurnInControl.Shared.ComDefinitions.Station;
namespace BurnInControl.Shared.ComDefinitions.MessagePacket;

public class MessagePacket<TPacket> where TPacket:IPacket{
    public StationMsgPrefix  Prefix { get; set; }
    public TPacket? Packet { get; set; }
}
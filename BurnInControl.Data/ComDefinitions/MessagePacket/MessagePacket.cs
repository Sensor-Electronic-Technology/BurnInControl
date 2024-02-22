using BurnIn.Data.ComDefinitions.Station;
namespace BurnIn.Data.ComDefinitions;

public class MessagePacket<TPacket> where TPacket:IPacket{
    public StationMsgPrefix  Prefix { get; set; }
    public TPacket? Packet { get; set; }
}
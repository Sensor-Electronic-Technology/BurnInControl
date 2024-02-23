using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
namespace BurnInControl.Application.StationControl.Messages;

public class SendSerialCommand:IStationControlMessage {
    public StationMsgPrefix MessagePrefix { get; set; }
    public IPacket Data { get; set; }
}
using BurnInControl.Shared.ComDefinitions.MessagePacket;
namespace BurnInControl.Shared.ComDefinitions.Packets;

public class StartTestFromPacket:IPacket {
    public string Message { get; set; }
    public string TestId { get; set; }
    public int SetCurrent { get; set; }
    public int SetTemperature { get; set; }
}
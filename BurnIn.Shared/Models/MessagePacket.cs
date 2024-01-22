using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
namespace BurnIn.Shared.Models;


public class MessagePacket {
    public ArduinoMsgPrefix Prefix { get; set; }
    public object Packet { get; set; }
}


public interface IMsgPacket<TValue> {
    public string Prefix { get; set; }
    public TValue Packet { get; set; }

    public string Serialize();
}

public class HeaterConfigMsgPacket : IMsgPacket<HeaterControllerConfig> {

    public string Prefix { get; set; }
    public HeaterControllerConfig Packet { get; set; }
    public String Serialize() {
        throw new NotImplementedException();
    }
}



/*public abstract class Serializable

public interface IMessagePacket {
    string Prefix { get; set; }
    Serializable Serialize();
}*/






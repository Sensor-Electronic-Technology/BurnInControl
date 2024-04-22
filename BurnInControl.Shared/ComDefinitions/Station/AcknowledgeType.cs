using System.Text.Json.Serialization;
using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;

namespace BurnInControl.Shared.ComDefinitions.Station;

/*enum AckType:uint8_t{
    TEST_START_ACK=0,
    VER_ACK=1,
    ID_ACK=2   
};*/
[JsonConverter(typeof(AcknowledgeTypeJsonConverter))]
public class AcknowledgeType(string name, byte value) : SmartEnum<AcknowledgeType, byte>(name, value), IPacket {
    public static readonly AcknowledgeType TestStartAck = new AcknowledgeType(nameof(TestStartAck), 0);
    public static readonly AcknowledgeType VersionAck = new AcknowledgeType(nameof(TestStartAck), 1);
    public static readonly AcknowledgeType IdAck = new AcknowledgeType(nameof(TestStartAck), 2);
}
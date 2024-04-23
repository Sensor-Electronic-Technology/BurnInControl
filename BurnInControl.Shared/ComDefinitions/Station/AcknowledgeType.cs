using System.Text.Json.Serialization;
using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;

namespace BurnInControl.Shared.ComDefinitions.Station;

[JsonConverter(typeof(AcknowledgeTypeJsonConverter))]
public class AcknowledgeType(string name, byte value) : SmartEnum<AcknowledgeType, byte>(name, value), IPacket {
    public static readonly AcknowledgeType TestStartAck = new AcknowledgeType(nameof(TestStartAck), 0);
    public static readonly AcknowledgeType VersionAck = new AcknowledgeType(nameof(VersionAck), 1);
    public static readonly AcknowledgeType IdAck = new AcknowledgeType(nameof(IdAck), 2);
    public static readonly AcknowledgeType TestCompleteAck = new AcknowledgeType(nameof(TestCompleteAck), 3);
}
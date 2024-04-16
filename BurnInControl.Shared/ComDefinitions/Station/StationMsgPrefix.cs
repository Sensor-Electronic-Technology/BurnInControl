using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.Station;

[JsonConverter(typeof(StationMsgPrefixJsonConverter))]
public class StationMsgPrefix : SmartEnum<StationMsgPrefix,string>,IPacket {
    public static readonly StationMsgPrefix HeaterConfigPrefix= new StationMsgPrefix(nameof(HeaterConfigPrefix), "CH"); 
    public static readonly StationMsgPrefix ProbeConfigPrefix = new StationMsgPrefix(nameof(ProbeConfigPrefix),"CP");
    public static readonly StationMsgPrefix StationConfigPrefix = new StationMsgPrefix(nameof(StationConfigPrefix), "CS");
    public static readonly StationMsgPrefix SaveState = new StationMsgPrefix(nameof(SaveState), "ST");
    public static readonly StationMsgPrefix MessagePrefix = new StationMsgPrefix(nameof(MessagePrefix), "M");
    public static readonly StationMsgPrefix DataPrefix = new StationMsgPrefix(nameof(DataPrefix), "D");
    public static readonly StationMsgPrefix CommandPrefix = new StationMsgPrefix(nameof(CommandPrefix), "COM");
    public static readonly StationMsgPrefix IdReceivePrefix = new StationMsgPrefix(nameof(IdReceivePrefix), "IDREC");
    public static readonly StationMsgPrefix IdRequestPrefix = new StationMsgPrefix(nameof(IdRequestPrefix), "IDREQ");
    public static readonly StationMsgPrefix VersionReceivePrefix = new StationMsgPrefix(nameof(VersionReceivePrefix), "VERREC");
    public static readonly StationMsgPrefix VersionRequestPrefix = new StationMsgPrefix(nameof(VersionRequestPrefix), "VERREQ");
    public static readonly StationMsgPrefix TestStatusPrefix = new StationMsgPrefix(nameof(TestStatusPrefix), "TSTAT");
    public static readonly StationMsgPrefix TestCompletedPrefix = new StationMsgPrefix(nameof(TestCompletedPrefix), "TCOMP");
    public static readonly StationMsgPrefix TestStartFromLoadPrefix = new StationMsgPrefix(nameof(TestStartFromLoadPrefix), "TLOAD");
    public static readonly StationMsgPrefix HeaterNotifyPrefix = new StationMsgPrefix(nameof(HeaterNotifyPrefix), "HNOTIFY");
    public static readonly StationMsgPrefix HeaterTuneCompletePrefix=new StationMsgPrefix(nameof(HeaterTuneCompletePrefix), "HTUNED");
    public static readonly StationMsgPrefix AcknowledgePrefix = new StationMsgPrefix(nameof(AcknowledgePrefix), "ACK");
    public static readonly StationMsgPrefix UpdateCurrentPrefix = new StationMsgPrefix(nameof(AcknowledgePrefix), "UC");
    public static readonly StationMsgPrefix UpdateTempPrefix = new StationMsgPrefix(nameof(AcknowledgePrefix), "UT");
    public static readonly StationMsgPrefix TuneComPrefix = new StationMsgPrefix(nameof(AcknowledgePrefix), "TCOM");
    private StationMsgPrefix(string name, string value) : base(name, value) {  }
}
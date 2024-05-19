using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.Station;

[JsonConverter(typeof(StationMsgPrefixJsonConverter))]
public class StationMsgPrefix : SmartEnum<StationMsgPrefix,string>,IPacket {
    public static readonly StationMsgPrefix HeaterConfigPrefix= new(nameof(HeaterConfigPrefix), "CH"); 
    public static readonly StationMsgPrefix ProbeConfigPrefix = new(nameof(ProbeConfigPrefix),"CP");
    public static readonly StationMsgPrefix StationConfigPrefix = new(nameof(StationConfigPrefix), "CS");
    public static readonly StationMsgPrefix SaveState = new(nameof(SaveState), "ST");
    public static readonly StationMsgPrefix MessagePrefix = new(nameof(MessagePrefix), "M");
    public static readonly StationMsgPrefix DataPrefix = new(nameof(DataPrefix), "D");
    public static readonly StationMsgPrefix CommandPrefix = new(nameof(CommandPrefix), "COM");
    public static readonly StationMsgPrefix IdReceivePrefix = new(nameof(IdReceivePrefix), "IDREC");
    public static readonly StationMsgPrefix IdRequestPrefix = new(nameof(IdRequestPrefix), "IDREQ");
    public static readonly StationMsgPrefix VersionReceivePrefix = new(nameof(VersionReceivePrefix), "VERREC");
    public static readonly StationMsgPrefix VersionRequestPrefix = new(nameof(VersionRequestPrefix), "VERREQ");
    public static readonly StationMsgPrefix TestStartStatusPrefix = new(nameof(TestStartStatusPrefix), "TSTAT");
    public static readonly StationMsgPrefix TestCompletedPrefix = new(nameof(TestCompletedPrefix), "TCOMP");
    public static readonly StationMsgPrefix TestStartFromLoadPrefix = new(nameof(TestStartFromLoadPrefix), "TLOAD");
    public static readonly StationMsgPrefix HeaterNotifyPrefix = new(nameof(HeaterNotifyPrefix), "HNOTIFY");
    public static readonly StationMsgPrefix HeaterTuneCompletePrefix=new(nameof(HeaterTuneCompletePrefix), "HTUNED");
    public static readonly StationMsgPrefix AcknowledgePrefix = new(nameof(AcknowledgePrefix), "ACK");
    public static readonly StationMsgPrefix UpdateCurrentPrefix = new(nameof(AcknowledgePrefix), "UC");
    public static readonly StationMsgPrefix UpdateTempPrefix = new(nameof(AcknowledgePrefix), "UT");
    public static readonly StationMsgPrefix TuneComPrefix = new(nameof(AcknowledgePrefix), "TCOM");
    public static readonly StationMsgPrefix SendTestIdPrefix = new(nameof(SendTestIdPrefix), "TID");
    public static readonly StationMsgPrefix LoadStatePrefix = new(nameof(LoadStatePrefix), "LSTATE");
    public static readonly StationMsgPrefix SaveConfigStatusPrefix= new(nameof(SaveConfigStatusPrefix), "SCONF");
    private StationMsgPrefix(string name, string value) : base(name, value) {  }
}
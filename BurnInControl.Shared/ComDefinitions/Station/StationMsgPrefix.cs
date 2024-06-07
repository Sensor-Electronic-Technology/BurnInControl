using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.JsonConverters;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using System.Text.Json.Serialization;
namespace BurnInControl.Shared.ComDefinitions.Station;

/*public enum StationMessageType {
    GENERAL,
    INIT,
    NOTIFY,
    ERROR,
}*/

/*public class StationMessageType : SmartEnum<StationMessageType, int>, IPacket {
    public static readonly StationMessageType General = new(nameof(StationMessageType), 0);
    public static readonly StationMessageType Init = new(nameof(StationMessageType), 0);
    public static readonly StationMessageType Notify = new(nameof(StationMessageType), 0);
    public static readonly StationMessageType Error = new(nameof(StationMessageType), 0);
    public StationMessageType(string name, int value) : base(name, value) { }
}*/

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
    public static readonly StationMsgPrefix UpdateCurrentPrefix = new(nameof(UpdateCurrentPrefix), "UC");
    public static readonly StationMsgPrefix UpdateTempPrefix = new(nameof(UpdateTempPrefix), "UT");
    public static readonly StationMsgPrefix TuneComPrefix = new(nameof(TuneComPrefix), "TCOM");
    public static readonly StationMsgPrefix SendTestIdPrefix = new(nameof(SendTestIdPrefix), "TID");
    public static readonly StationMsgPrefix LoadStatePrefix = new(nameof(LoadStatePrefix), "LSTATE");
    public static readonly StationMsgPrefix SaveConfigStatusPrefix= new(nameof(SaveConfigStatusPrefix), "SCONF");
    public static readonly StationMsgPrefix GetConfigPrefix=new(nameof(GetConfigPrefix), "GCONF");
    public static readonly StationMsgPrefix ReceiveConfigPrefix=new(nameof(ReceiveConfigPrefix), "RCONF");
    /*public static readonly StationMsgPrefix FormatSdPrefix=new(nameof(FormatSdPrefix), "FSD");*/
    public static readonly StationMsgPrefix ProbeTestDonePrefix = new(nameof(ProbeTestDonePrefix), "PTD");
    public static readonly StationMsgPrefix SendRunningTestPrefix = new(nameof(SendRunningTestPrefix), "RTEST");
    public static readonly StationMsgPrefix NotifyHeaterModePrefix = new(nameof(NotifyHeaterModePrefix), "SWHEATER");
    public static readonly StationMsgPrefix SendTuneWindowSizePrefix = new(nameof(SendTuneWindowSizePrefix), "WINSIZE");
    private StationMsgPrefix(string name, string value) : base(name, value) {  }
}
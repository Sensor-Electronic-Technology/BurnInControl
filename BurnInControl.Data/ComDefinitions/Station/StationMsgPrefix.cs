using Ardalis.SmartEnum;
using BurnIn.Data.ComDefinitions.JsonConverters;
using System.Text.Json.Serialization;
namespace BurnIn.Data.ComDefinitions.Station;

[JsonConverter(typeof(StationMsgPrefixJsonConverter))]
public class StationMsgPrefix : SmartEnum<StationMsgPrefix,string>,IPacket {
    public static readonly StationMsgPrefix HeaterConfigPrefix= new StationMsgPrefix(nameof(HeaterConfigPrefix), "CH");
    public static readonly StationMsgPrefix ProbeConfigPrefix = new StationMsgPrefix(nameof(ProbeConfigPrefix),"CP");
    public static readonly StationMsgPrefix StationConfigPrefix = new StationMsgPrefix(nameof(StationConfigPrefix), "CS");
    public static readonly StationMsgPrefix SaveState = new StationMsgPrefix(nameof(SaveState), "ST");
    public static readonly StationMsgPrefix MessagePrefix = new StationMsgPrefix(nameof(MessagePrefix), "M");
    public static readonly StationMsgPrefix DataPrefix = new StationMsgPrefix(nameof(DataPrefix), "D");
    public static readonly StationMsgPrefix CommandPrefix = new StationMsgPrefix(nameof(CommandPrefix), "COM");
    public static readonly StationMsgPrefix HeaterResponse = new StationMsgPrefix(nameof(HeaterResponse), "HRES");
    public static readonly StationMsgPrefix TestResponse = new StationMsgPrefix(nameof(TestResponse), "TRES");
    public static readonly StationMsgPrefix HeaterRequest = new StationMsgPrefix(nameof(HeaterResponse), "HREQ");
    public static readonly StationMsgPrefix TestRequest = new StationMsgPrefix(nameof(TestResponse), "TREQ");
    public static readonly StationMsgPrefix IdReceive = new StationMsgPrefix(nameof(IdReceive), "IDREC");
    public static readonly StationMsgPrefix IdRequest = new StationMsgPrefix(nameof(IdRequest), "IDREQ");
    public static readonly StationMsgPrefix VersionReceive = new StationMsgPrefix(nameof(VersionReceive), "VERREC");
    public static readonly StationMsgPrefix VersionRequest = new StationMsgPrefix(nameof(VersionRequest), "VERREQ");
    public static readonly StationMsgPrefix InitMessage = new StationMsgPrefix(nameof(InitMessage), "INIT");
    public static readonly StationMsgPrefix TestStatus = new StationMsgPrefix(nameof(TestStatus), "TSTAT");
    private StationMsgPrefix(string name, string value) : base(name, value) {  }
}
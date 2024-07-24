using MongoDB.Bson;

namespace BurnInControl.Data.BurnInTests.DataTransfer;

public class WaferTestResultDto {
    public ObjectId TestLogId { get; set; }
    public string WaferId { get; set; }
    public string Pocket { get; set; }
    public WaferProbeData? Probe1Data { get; set; }
    public WaferProbeData? Probe2Data { get; set; }
}

public class WaferProbeData {
    public string PadId { get; set; }
    public ulong RunTime { get; set; }
    public double InitCurrent { get; set; }
    public double InitVoltage { get; set; }
    public double FinalCurrent { get; set; }
    public double FinalVoltage { get; set; }
    public bool Okay { get; set; }
    
}
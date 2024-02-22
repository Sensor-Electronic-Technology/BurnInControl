namespace BurnIn.Data.ComDefinitions.Packets;

public class StartTestStatus : IPacket {
    public bool Success { get; set; }
    public string Message { get; set; }
    public StartTestStatus() {
        //for json serialization
    }
}
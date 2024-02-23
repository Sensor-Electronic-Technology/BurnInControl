using BurnInControl.Shared.ComDefinitions.MessagePacket;
using ErrorOr;
namespace BurnInControl.Shared.ComDefinitions.Packets;

/*public class StartTestStatus : IPacket {
    public bool Success { get; set; }
    public string Message { get; set; }
    public StartTestStatus() {
        //for json serialization
    }
}*/

public class StartTestStatus {
    public ErrorOr<Success> Status { get; set; }
}
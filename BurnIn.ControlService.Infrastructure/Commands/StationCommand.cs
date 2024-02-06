using BurnIn.Shared.Models;
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Commands;

public enum StationCommand {
    ConnectUsb,
    DisconnectUsb,
    SendCommand,
    RequestId,
    CheckUpdateAvailable,
    SetupTest
}

public class StationRequest<TPacket>:IRequest where TPacket:IPacket{
    public StationCommand Command { get; set; }
    public MessagePacketV2<TPacket> Packet { get; set; }
}

/*public class StationCommand:IRequest {
    public ArduinoCommand ArduinoCommand { get; set; }
}*/
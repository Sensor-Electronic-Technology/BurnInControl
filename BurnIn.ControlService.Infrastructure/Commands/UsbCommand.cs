using MediatR;
namespace BurnIn.ControlService.Infrastructure.Commands;

public enum UsbCommand {
    Connect,
    Disconnect
}

public class SerialCommand:IRequest {
    public UsbCommand Command { get; set; }
}
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Commands;

public class ProcessSerialCommand:IRequest {
    public string? Message { get; set; }
}
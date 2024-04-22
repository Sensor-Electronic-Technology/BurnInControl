using ErrorOr;
using MediatR;
namespace BurnInControl.Application.BurnInTest.Messages;

public class StartStatusCommand:IRequest {
    public bool Status { get; set; }
    public string? Message { get; set; }
    public string? TestId { get; set; }
}


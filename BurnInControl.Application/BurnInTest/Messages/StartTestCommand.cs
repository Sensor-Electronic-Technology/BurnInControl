using ErrorOr;
using MediatR;
namespace BurnInControl.Application.BurnInTest.Messages;

public class StartTestCommand:IRequest {
    public bool Status { get; set; }
    public string? Message { get; set; }
}


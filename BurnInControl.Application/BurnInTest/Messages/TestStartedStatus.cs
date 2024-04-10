using ErrorOr;
using MediatR;
namespace BurnInControl.Application.BurnInTest.Messages;

public class TestStartedStatus:IBurnInTestMessage, IRequest {
    public bool Status { get; set; }
}
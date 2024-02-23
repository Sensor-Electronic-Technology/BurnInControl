using ErrorOr;
namespace BurnInControl.Application.BurnInTest.Messages;

public class TestStartedStatus:IBurnInTestMessage {
    public ErrorOr<Success> Status { get; set; }
}
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SendTestIdCommand : IRequest {
    public string TestId { get; set; }
}
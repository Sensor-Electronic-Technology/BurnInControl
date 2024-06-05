using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SendTuningWindowSizeCommand:IRequest {
    public int WindowSize { get; set; }
}
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class UpdateCurrentTempCommand:IRequest {
    public int Current { get; set; }
    public int Temperature { get; set; }
}
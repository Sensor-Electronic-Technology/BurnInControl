using BurnInControl.Data.BurnInTests;
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SendSavedStateCommand : IRequest {
    public ControllerSavedState SavedState { get; set; }
}
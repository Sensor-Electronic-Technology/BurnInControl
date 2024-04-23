using BurnInControl.Data.ComponentConfiguration.ProbeController;
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SendProbeConfig:IRequest {
    public ProbeControllerConfig ProbeConfig { get; set; }
}
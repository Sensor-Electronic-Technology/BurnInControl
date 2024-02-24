using BurnInControl.Data.ComponentConfiguration.ProbeController;
namespace BurnInControl.Application.StationControl.Messages;

public class SendProbeConfig {
    public ProbeControllerConfig ProbeConfig { get; set; }
}
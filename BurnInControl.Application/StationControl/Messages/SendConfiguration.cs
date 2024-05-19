using BurnInControl.Data.ComponentConfiguration;
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SendConfiguration : IRequest {
    public IBurnStationConfiguration? Configuration { get; set; }
}
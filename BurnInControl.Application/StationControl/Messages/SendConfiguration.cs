using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SendConfiguration : IRequest {
    public IBurnStationConfiguration? Configuration { get; set; }
    //public ConfigType ConfigType { get; set; }
}
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class RequestConfigMessage : IRequest {
    public ConfigType ConfigType { get; set; }
}
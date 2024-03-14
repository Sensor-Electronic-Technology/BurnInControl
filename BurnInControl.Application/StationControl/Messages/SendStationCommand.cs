using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.StationControl.Messages;

public class SendStationCommand:IRequest<ErrorOr<Success>> {
    public StationCommand Command { get; set; }
}
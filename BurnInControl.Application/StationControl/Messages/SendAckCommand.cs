using Ardalis.SmartEnum;
using BurnInControl.Shared.ComDefinitions.Station;
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SendAckCommand : IRequest {
    public AcknowledgeType AcknowledgeType { get; set; }
}
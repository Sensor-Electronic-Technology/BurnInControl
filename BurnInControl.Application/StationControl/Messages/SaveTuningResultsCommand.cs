using BurnInControl.Shared.ComDefinitions;
using MediatR;

namespace BurnInControl.Application.StationControl.Messages;

public class SaveTuningResultsCommand:IRequest {
    public List<HeaterTuneResult> Results { get; set; }
}
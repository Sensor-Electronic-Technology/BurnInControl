using BurnIn.Shared;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.StationData;
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Commands;

public class TestSetupCommand:IRequest<Result> {
    public List<WaferSetup> TestSetup { get; set; }
}

public class LogCommand:IRequest<Result> {
    public StationSerialData Data { get; set; }
}

public class TestStartedStatus : INotification {
    public Result Status { get; set; }
}




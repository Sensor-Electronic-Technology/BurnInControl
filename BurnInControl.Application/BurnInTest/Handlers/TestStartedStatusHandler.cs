using BurnInControl.Application.BurnInTest.Messages;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.BurnInTest.Handlers;

public class TestStartedStatusHandler:IRequestHandler<TestStartedStatus> {
    private readonly IBurnInTestService _burnInService;
    public TestStartedStatusHandler(IBurnInTestService burnInService) {
        this._burnInService= burnInService;
    }

    public Task Handle(TestStartedStatus request, CancellationToken cancellationToken) {
        this._burnInService.StartTestLogging();
        return Task.CompletedTask;
    }
}
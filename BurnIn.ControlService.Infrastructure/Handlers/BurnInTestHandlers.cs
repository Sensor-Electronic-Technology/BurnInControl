using BurnIn.ControlService.Infrastructure.Commands;
using BurnIn.Shared;
using BurnIn.Shared.Services;
using MediatR;
namespace BurnIn.ControlService.Infrastructure.Handlers;

public class TestSetupHandler : IRequestHandler<TestSetupCommand,Result> {
    private readonly BurnInTestService _testService;
    public TestSetupHandler(BurnInTestService testService) {
        this._testService = testService;
    }
    public Task<Result> Handle(TestSetupCommand request, CancellationToken cancellationToken) {
        return Task.FromResult(this._testService.SetupTest(request.TestSetup));
    }
}

public class TestLogCommandHandler : IRequestHandler<LogCommand,Result> {
    private readonly BurnInTestService _testService;
    public TestLogCommandHandler(BurnInTestService testService) {
        this._testService = testService;
    }
    public Task<Result> Handle(LogCommand request, CancellationToken cancellationToken) {
        return Task.FromResult(this._testService.Log(request.Data));
    }
}

public class TestStartedStatusHandler : INotificationHandler<TestStartedStatus> {
    private readonly BurnInTestService _testService;
    public TestStartedStatusHandler(BurnInTestService testService) {
        this._testService = testService;
    }

    public Task Handle(TestStartedStatus notification, CancellationToken cancellationToken) {
        if (notification.Status.IsSuccess) {
            this._testService.StartTestLogging();
        } else {
            this._testService.StartTestLogging();
        }
        return Task.CompletedTask;
    }
}
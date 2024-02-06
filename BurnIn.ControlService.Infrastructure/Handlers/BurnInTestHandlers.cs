using BurnIn.ControlService.Infrastructure.Commands;
using BurnIn.Shared.Services;
using MediatR;
namespace BurnIn.Shared.Handlers;

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

public class TestStartedNotifyHandler : INotificationHandler<TestStartedNotify> {
    private readonly BurnInTestService _testService;
    public TestStartedNotifyHandler(BurnInTestService testService) {
        this._testService = testService;
    }

    public Task Handle(TestStartedNotify notification, CancellationToken cancellationToken) {
        this._testService.SetSetupComplete();
        return Task.CompletedTask;
    }
}
using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class StartFromLoadCommandHandler : IRequestHandler<StartFromLoadCommand> {
    private readonly ITestService _testService;
    public StartFromLoadCommandHandler(ITestService testService) {
        this._testService = testService;
    }
    public Task Handle(StartFromLoadCommand request, CancellationToken cancellationToken) {
        return this._testService.StartFrom(request.SavedState);
    }
}
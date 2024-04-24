using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class StartLoadStateCommandHandler : IRequestHandler<StartLoadStateCommand> {
    private readonly ITestService _testService;

    public StartLoadStateCommandHandler(ITestService testService) {
        this._testService = testService;
    }
    public Task Handle(StartLoadStateCommand request, CancellationToken cancellationToken) {
        return this._testService.LoadState(request.LogId);
    }
} 
    

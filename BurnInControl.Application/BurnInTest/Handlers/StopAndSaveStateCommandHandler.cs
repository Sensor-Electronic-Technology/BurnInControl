using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class StopAndSaveStateCommandHandler:IRequestHandler<StopAndSaveStateCommand>{
    private readonly ITestService _testService;

    public StopAndSaveStateCommandHandler(ITestService testService, IMediator mediator) {
        _testService = testService;
    }

    public async Task Handle(StopAndSaveStateCommand request, CancellationToken cancellationToken) {
        await _testService.StopAndSaveState();
    }
}
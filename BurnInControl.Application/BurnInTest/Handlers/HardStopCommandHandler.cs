using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class HardStopCommandHandler:IRequestHandler<HardStopCommand> {
    private readonly ITestService _testService;
    public HardStopCommandHandler(ITestService testService) {
        _testService = testService;
    }
    public Task Handle(HardStopCommand request, CancellationToken cancellationToken) {
        return this._testService.Stop();
    }
}
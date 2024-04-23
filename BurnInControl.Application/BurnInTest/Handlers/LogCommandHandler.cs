using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class LogCommandHandler: IRequestHandler<LogCommand> {
    private readonly ITestService _testService;

    public LogCommandHandler(ITestService testService) {
        this._testService = testService;
    }
    
    public Task Handle(LogCommand request, CancellationToken cancellationToken) {
        return this._testService.Log(request.Data);
    }
}
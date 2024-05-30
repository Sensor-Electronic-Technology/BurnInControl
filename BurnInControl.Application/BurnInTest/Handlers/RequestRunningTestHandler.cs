using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class RequestRunningTestHandler:IRequestHandler<RequestRunningTestCommand> {
    private readonly ITestService _testService;

    public RequestRunningTestHandler(ITestService testService) {
        this._testService = testService;
    }
    
    public Task Handle(RequestRunningTestCommand request, CancellationToken cancellationToken) {
        Console.WriteLine("Sending Running Test");
        return this._testService.SendRunningTest();
    }
}
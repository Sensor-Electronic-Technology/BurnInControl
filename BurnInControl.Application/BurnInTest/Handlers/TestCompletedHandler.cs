using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class TestCompletedHandler:IRequestHandler<TestCompleteCommand> {
    private ITestService _testService;
    
    public TestCompletedHandler(ITestService testService) {
        this._testService = testService;
    }
    public Task Handle(TestCompleteCommand request, CancellationToken cancellationToken) {
        return this._testService.StopTest();
    }
}
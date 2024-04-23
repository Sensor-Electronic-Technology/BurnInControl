using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class TestSetupHandler:IRequestHandler<TestSetupCommand> {
    private readonly ITestService _testService;
    public TestSetupHandler(ITestService testService) {
        this._testService = testService;
    }
    
    public Task Handle(TestSetupCommand request, CancellationToken cancellationToken) {
        return this._testService.SetupTest(request.TestSetupTransport);
    }
}
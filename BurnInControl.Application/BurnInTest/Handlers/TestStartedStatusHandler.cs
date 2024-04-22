using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.BurnInTest.Handlers;

public class TestStartedStatusHandler:IRequestHandler<StartStatusCommand>,IRequestHandler<StartFromLoadCommand> {
    private readonly ITestService _testService;
    public TestStartedStatusHandler(ITestService testService) {
        this._testService = testService;
    }

    public Task Handle(StartStatusCommand request, CancellationToken cancellationToken) {
        return this._testService.Start(request.Status,request.TestId,request.Message);
    }

    public Task Handle(StartFromLoadCommand request, CancellationToken cancellationToken) {
        return this._testService.StartFrom(request.Message, request.TestId,request.Current,request.SetTemperature);
    }
}
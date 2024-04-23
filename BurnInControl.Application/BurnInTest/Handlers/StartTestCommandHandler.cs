using BurnInControl.Application.BurnInTest.Interfaces;
using BurnInControl.Application.BurnInTest.Messages;
using ErrorOr;
using MediatR;
namespace BurnInControl.Application.BurnInTest.Handlers;

public class StartTestCommandHandler:IRequestHandler<StartTestCommand>,IRequestHandler<StartFromLoadCommand> {
    private readonly ITestService _testService;
    public StartTestCommandHandler(ITestService testService) {
        this._testService = testService;
    }

    public Task Handle(StartTestCommand request, CancellationToken cancellationToken) {
        return this._testService.Start(request.Status,request.Message);
    }
    
    public Task Handle(StartFromLoadCommand request, CancellationToken cancellationToken) {
        return this._testService.StartFrom(request.Message, request.TestId,request.Current,request.SetTemperature);
    }
}
using BurnInControl.Data.BurnInTests.Wafers;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Messages;

public class TestSetupCommand:IRequest {
    public List<WaferSetup> TestSetup { get; set; }
}
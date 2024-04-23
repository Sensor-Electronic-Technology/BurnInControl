using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.HubDefinitions.HubTransports;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Messages;

public class TestSetupCommand:IRequest {
    public TestSetupTransport TestSetupTransport { get; set; }
}
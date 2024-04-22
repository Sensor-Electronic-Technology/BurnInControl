using BurnInControl.Shared.ComDefinitions;
using MediatR;

namespace BurnInControl.Application.BurnInTest.Messages;

public class LogCommand:IRequest{
    public StationSerialData Data { get; set; }
}
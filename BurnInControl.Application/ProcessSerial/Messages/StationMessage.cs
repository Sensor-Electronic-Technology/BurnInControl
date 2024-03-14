using MediatR;
namespace BurnInControl.Application.ProcessSerial.Messages;

public class StationMessage:IRequest {
    public string Message { get; set; }
}
using BurnInControl.Shared.ComDefinitions;
namespace BurnInControl.Application.BurnInTest.Messages;

public class LogCommand:IBurnInTestMessage{
    public StationSerialData Data { get; set; }
}
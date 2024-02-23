using BurnInControl.Data.BurnInTests.Wafers;
namespace BurnInControl.Application.BurnInTest.Messages;

public class TestSetupCommand:IBurnInTestMessage {
    public List<WaferSetup> TestSetup { get; set; }
}
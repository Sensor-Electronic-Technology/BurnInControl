using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;

namespace BurnInControl.Application.BurnInTest.Handlers;

public class TestSetupHandler {
    public List<WaferSetup> WaferSetups { get; set; }
    public StationCurrent SetCurrent { get; set; }
    public int SetTemperature { get; set; }
}
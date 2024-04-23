using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;

namespace BurnInControl.HubDefinitions.HubTransports;

public class TestSetupTransport {
    public List<WaferSetup> WaferSetups { get; set; }
    public StationCurrent SetCurrent { get; set; }
    public int SetTemperature { get; set; }

    public TestSetupTransport() {
        //for json de/serialization
    }
}
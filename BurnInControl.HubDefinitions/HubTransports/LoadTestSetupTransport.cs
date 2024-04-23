using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel.Components;

namespace BurnInControl.HubDefinitions.HubTransports;

public class LoadTestSetupTransport {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<WaferSetup>? WaferSetups { get; set; }
    public StationCurrent? SetCurrent { get; set; }
    public int SetTemperature { get; set; }
}
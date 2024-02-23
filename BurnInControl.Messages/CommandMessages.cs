using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
namespace BurnInControl.Messages;

public interface IExternalCommand{};


public record ConnectUsb : IExternalCommand;
public record DisconnectUsb:IExternalCommand;

public record SendCommand(StationCommand Command):IExternalCommand;
public record SendId(string newId):IExternalCommand;
public record RequestId:IExternalCommand;
public record SetupTest(IList<WaferSetup> testSetup):IExternalCommand;
public record StartTest:IExternalCommand;
public record UpdateFirmware:IExternalCommand;
public record CheckForUpdate:IExternalCommand;
public record SendProbeConfig(ProbeControllerConfig packet):IExternalCommand;
public record SendHeaterConfig(HeaterControllerConfig packet):IExternalCommand;
public record SendStationConfig(StationConfiguration packet):IExternalCommand;
public record SendFirmwareVersion(string newVersion):IExternalCommand;




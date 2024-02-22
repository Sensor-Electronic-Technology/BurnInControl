using BurnIn.Data.ComDefinitions;
using BurnIn.Data.ComDefinitions.Station;
using BurnIn.Data.ComponentConfiguration.HeaterController;
using BurnIn.Data.ComponentConfiguration.ProbeController;
using BurnIn.Data.ComponentConfiguration.StationController;
using BurnIn.Data.Station.Configuration;
using BurnIn.Data.StationModel.TestLogs;
using BurnIn.Data.StationModel.TestLogs.Wafers;
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




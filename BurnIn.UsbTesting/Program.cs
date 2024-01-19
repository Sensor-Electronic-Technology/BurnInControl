// See https://aka.ms/new-console-template for more information
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using CP.IO.Ports;
using ReactiveMarbles.Extensions;
using System.Collections.Concurrent;

using System.Diagnostics;
using System.IO.Ports;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Controller;
using BurnIn.Shared.Services;
using BurnIn.UsbTesting;
using ConsoleTools;
using MongoDB.Bson;
using MongoDB.Driver;
using DataReceivedEventArgs=System.Diagnostics.DataReceivedEventArgs;
using StationConfiguration=BurnIn.Shared.Models.Configurations.StationConfiguration;


//await CreateAndOutputProbeConfigFile();
//await CreateStationDatabase("S01");
RunControllerTests();
void RunControllerTests() {
    ControllerTests tests = new ControllerTests();
    tests.Setup();
    if (tests.Connect()) {
        tests.Run();
    } else {
        Console.WriteLine("Goodbye!!");
    }
}

async Task CreateStationDatabase(string stationId) {
    Console.WriteLine($"Creating Station Config Database.  Config for {stationId}");
    var client=new MongoClient("mongodb://172.20.3.41");
    StationConfigurationService configService = new StationConfigurationService(client);
    BurnStationConfiguration configuration = new BurnStationConfiguration();
    configuration.StationId = stationId;
    configuration.StationPosition = "P20";
    configuration.StationConfiguration = CreateStationConfig();
    configuration.ProbesConfiguration = CreateProbeControlConfig();
    configuration.HeaterConfig = CreateHeaterControlConfig();
    configuration._id = ObjectId.GenerateNewId();
    await configService.InsertConfiguration(configuration);
    Console.WriteLine("Check Database");
}

StationConfiguration CreateStationConfig() {
    var configuration =new StationConfiguration(1000, 500, 60000);
    var burnTimerConfig = new BurnTimerConfig(72000, 72000, 25200);
    configuration.BurnTimerConfig = burnTimerConfig;
    return configuration;
}

ProbeControllerConfig CreateProbeControlConfig() {
    ProbeControllerConfig probeControllerConfig = new ProbeControllerConfig() { 
        CurrentSelectConfig=new CurrentSelectorConfig(2,6,7,60,true),
        CurrentPercent = .80,
        ProbeTestCurrent = 60,
        ReadInterval = 100,
        ProbeConfigurations = {
            new ProbeConfig(new VoltageSensorConfig(54, 0.1), new CurrentSensorConfig(63, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(55, 0.1), new CurrentSensorConfig(64, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(56, 0.1), new CurrentSensorConfig(65, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(57, 0.1), new CurrentSensorConfig(66, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(58, 0.1), new CurrentSensorConfig(67, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(59, 0.1), new CurrentSensorConfig(68, 0.1))
        }
    };
    return probeControllerConfig;
}

HeaterControllerConfig CreateHeaterControlConfig() {
    NtcConfiguration ntcConfig1 = new NtcConfiguration(1.159e-3f, 1.429e-4f, 1.118e-6f, 60, 0.01);
    NtcConfiguration ntcConfig2 = new NtcConfiguration(1.173e-3f, 1.736e-4f, 7.354e-7f, 61, 0.01);
    NtcConfiguration ntcConfig3 = new NtcConfiguration(1.200e-3f, 1.604e-4f, 8.502e-7f, 62, 0.01);

    PidConfiguration pidConfig1 = new PidConfiguration(242.21,1868.81,128.49,250);
    PidConfiguration pidConfig2 = new PidConfiguration(765.77,1345.82,604.67,250);
    PidConfiguration pidConfig3 = new PidConfiguration(179.95,2216.84,81.62,250);

    HeaterConfiguration heaterConfig1 = new HeaterConfiguration(ntcConfig1, pidConfig1, 5, 3, 1);
    HeaterConfiguration heaterConfig2 = new HeaterConfiguration(ntcConfig2, pidConfig2, 5, 4, 2);
    HeaterConfiguration heaterConfig3 = new HeaterConfiguration(ntcConfig3, pidConfig3, 5, 5, 3);

    HeaterControllerConfig config = new HeaterControllerConfig();
    config.HeaterConfigurations = [
        heaterConfig1,
        heaterConfig1,
        heaterConfig2
    ];
    config.ReadInterval = 250;
    return config;
}
public record ArduinoSerialData {
    public List<double> Voltages { get; set; } = new List<Double>();
    public List<double> Currents { get; set; } = new List<Double>();
    public List<double> Temperatures { get; set; } = new List<Double>();
    public List<long> ProbeRuntimes { get; set; } = new List<Int64>();
    public bool Heater1State { get; set; }
    public bool Heater2State { get; set; }
    public bool Heater3State { get; set; }
    public int CurrentSetPoint { get; set; }
    public int TemperatureSetPoint { get; set; }
    public long RuntimeSeconds { get; set; }
    public long ElapsedSeconds { get; set; }
    public bool Running { get; set; }
    public bool Paused { get; set; }
}




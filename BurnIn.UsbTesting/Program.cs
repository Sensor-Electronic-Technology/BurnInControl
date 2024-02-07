// See https://aka.ms/new-console-template for more information
using BurnIn.ControlService.Infrastructure.Services;
using BurnIn.Shared.Models;
using System.Text.Json;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Models.StationData;
using BurnIn.Shared.Services;
using BurnIn.UsbTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using Octokit;
using System.Text.RegularExpressions;
using FileMode=System.IO.FileMode;



//await CreateAndOutputProbeConfigFile();
//await CreateStationDatabase("S01");
//RunControllerTests();
//RunUsbControllerTests();

//TestMessageCasting();
//await TestGithubClient();
/*Console.WriteLine("Checking With V01");
TestVersionCheck("V01");

Console.WriteLine("Checking With V2.01");
TestVersionCheck("V2.01");

Console.WriteLine("Checking With P01");
TestVersionCheck("P01");

Console.WriteLine("Checking With V1.02");
TestVersionCheck("V1.02");*/

/*Console.WriteLine("Create Station Database");
await CreateStationDatabase();*/
Console.WriteLine("Testing logs");
await TestLogDataServiceTesting();
Console.WriteLine("Test Completed");
/*Console.WriteLine("Stopped without marking complete, Press any key to continue test");
Console.ReadKey();
await ContinueFrom();*/

async Task CreateStationDatabase() {
    StationDataService service = new StationDataService(new MongoClient("mongodb://172.20.3.41:27017/"));
    TestConfiguration config150 = new TestConfiguration() {
        _id = ObjectId.GenerateNewId(),
        TestName = "150mA_7hrs",
        SetCurrent = StationCurrent._150mA,
        RunTime = 25200,
    };
    
    TestConfiguration config120 = new TestConfiguration() {
        _id = ObjectId.GenerateNewId(),
        TestName = "120mA_20Hrs",
        SetCurrent = StationCurrent._120mA,
        RunTime = 25200,
    };
    
    TestConfiguration config60 = new TestConfiguration() {
        _id = ObjectId.GenerateNewId(),
        TestName = "60mA_20Hrs",
        SetCurrent = StationCurrent._60mA,
        RunTime = 25200,
    };
    await service.InsertTestConfiguration(config150);
    await service.InsertTestConfiguration(config120);
    await service.InsertTestConfiguration(config60);
    Console.WriteLine("Test Configuration Created, Creating Stations");
    Station station = new Station();
    station.StationId = "S01";
    station.StationPosition = "P20";
    station.FirmwareVersion = "V1.1.2";
    station.State= StationState.Idle;
    station.Configuration = new BurnStationConfiguration() {
        HeaterConfig = CreateHeaterControlConfig(),
        ProbesConfiguration = CreateProbeControlConfig(),
        StationConfiguration = CreateStationConfig()
    };
    await service.InsertStation(station);
    Console.WriteLine("Station Created");
}

async Task TestLogDataServiceTesting(CancellationToken token=default) {
    var client = new MongoClient("mongodb://172.20.3.41:27017/");
    StationDataService stationService = new StationDataService(client);
    TestLogDataService service = new TestLogDataService(client,stationService);
    BurnInTestLog log = new BurnInTestLog();
    log.StationId = "S01";
    var testSetup=new List<WaferSetup>() {
        new WaferSetup() {
            WaferId = "W01",
            BurnNumber = 1,
            Probe1=StationProbe.Probe1,
            Probe2=StationProbe.Probe2,
            Probe1Pad = "a",
            Probe2Pad = "l"
        },
        new WaferSetup() {
            WaferId = "3",
            BurnNumber = 1,
            Probe1=StationProbe.Probe3,
            Probe2=StationProbe.Probe4,
            Probe1Pad = "a",
            Probe2Pad = "l"
        },
        new WaferSetup() {
            WaferId = "W03",
            BurnNumber = 1,
            Probe1=StationProbe.Probe5,
            Probe2=StationProbe.Probe6,
            Probe1Pad = "a",
            Probe2Pad = "l"
        }
    };
    log.StartNew(testSetup);
    await service.StartNew(log, StationCurrent._150mA);
    int count = 0;
    PeriodicTimer timer=new PeriodicTimer(new TimeSpan(0,0,0,0,500));
    CancellationTokenSource source = new CancellationTokenSource();
    var tokenSource = source.Token;
    await service.SetStart(log._id,DateTime.Now,GenerateSerialData());
    while(await timer.WaitForNextTickAsync(source.Token) && count<10) {
        var data = GenerateSerialData();
        log.AddReading(data);
        await service.InsertReading(log._id, data);
        count++;
        Console.WriteLine($"Logged Data, Log Count: {count}");
    }

    await service.SetCompleted(log._id,log.StationId,DateTime.Now);
    Console.WriteLine("Test Complete");
}

async Task ContinueFrom() {
    var client = new MongoClient("mongodb://172.20.3.41:27017/");
    StationDataService stationService = new StationDataService(client);
    TestLogDataService service = new TestLogDataService(client,stationService);
    var log=await service.CheckContinue("S01");
    if (log != null) {
        int count = 0;
        PeriodicTimer timer=new PeriodicTimer(new TimeSpan(0,0,0,0,500));
        CancellationTokenSource source = new CancellationTokenSource();
        var tokenSource = source.Token;
        while(await timer.WaitForNextTickAsync(source.Token) && count<10) {
            var data = GenerateSerialData();
            log.AddReading(data);
            await service.InsertReading(log._id, data);
            count++;
            Console.WriteLine($"Logged Data, Log Count: {count}");
        }

        await service.SetCompleted(log._id,log.StationId,DateTime.Now);
        Console.WriteLine("Test Complete");
    } else {
        Console.WriteLine("Test not found");
    }
}

StationSerialData GenerateSerialData() {
    StationSerialData data = new StationSerialData();
    for (int i = 0; i < 6; i++) {
        data.Voltages[i]=5.0;
        data.Currents[i] = 1.0;
        if (i < 3) {
            data.Temperatures[i] = 85;
        }
    }
    return data;
}

void TestVersionCheck(string fromController) {
    string latest = "V1.02";
    Regex regex = new Regex("^V[0-9]+\\.\\d\\d$", RegexOptions.IgnoreCase);
    var controlMatch=regex.IsMatch(fromController);
    var latestMatch = regex.IsMatch(latest);
    if (!controlMatch || !latestMatch) {
        string msg = (!controlMatch) ? 
            $"Controller version doesn't fit version pattern, Correct: V#.## Latest: {fromController}" 
            : $"Latest doesn't fit version pattern, Correct: V#.## Latest: {latest}";
        Console.WriteLine(msg);
        return;
    }
    if (latest == fromController) {
        Console.WriteLine("Versions are the same, No updates available");
        return;
    }
    string control = fromController.ToUpper();
    latest = latest.ToUpper();
    var latestSpan = latest.AsSpan();
    var controlSpan = control.AsSpan();
    bool majorMatch = false;
    bool sub1Match = false;
    bool sub2Match = false;
    for (int i = 0; i < latestSpan.Length;i++) {
        if (i == 1 || i==3 || i==4) {
            int latestV = Convert.ToInt16(latestSpan[i]);
            int controlV = Convert.ToInt16(controlSpan[i]);
            if (latestV > controlV) {
                if (i == 1) {
                    Console.WriteLine("Success! New Major Version Available");
                    break;
                } 
                if (i == 3 || i==4) {
                    Console.WriteLine($"Success! New Sub{i-2} Version Available");
                    break;
                }
            } else {
                Console.WriteLine(" are the same");
            }
        }
    }
}

async Task TestGithubClient() {
    var github = new GitHubClient(new ProductHeaderValue("Sensor-Electronic-Technology"));
    var result=await github.Repository.Release.GetLatest("Sensor-Electronic-Technology", "BurnInFirmware");
    Console.WriteLine($"Tag: {result.TagName}");
    var release = await github.Repository.Release.Get("Sensor-Electronic-Technology", "BurnInFirmware", result.TagName);
    HttpClient client = new HttpClient();

    var uri = new Uri(release.Assets[0].BrowserDownloadUrl);
    var stream = await client.GetStreamAsync(uri);
    Console.WriteLine($"Location: {Environment.CurrentDirectory.ToString()}");
    await using var fs = new FileStream(@"\\192.168.68.112\source\ControlUpload\"+release.Assets[0].Name, FileMode.Create);
    await stream.CopyToAsync(fs);
}

void TestDeserialize() {
    var probeConfig = CreateProbeControlConfig();
    MessagePacketV2<ProbeControllerConfig> msgPacket = new MessagePacketV2<ProbeControllerConfig>() {
        Prefix = ArduinoMsgPrefix.ProbeConfigPrefix,
        Packet=probeConfig
    };
    var output = JsonSerializer.Serialize(msgPacket);
    
}

void TestMessageCasting() {
    var probeConfig = CreateProbeControlConfig();
    MessagePacket msgPacket = new MessagePacket() {
        Prefix = ArduinoMsgPrefix.ProbeConfigPrefix,
        Packet=probeConfig
    };
    Parse(msgPacket);
    
}

void Parse(MessagePacket msgPacket) {
    if (msgPacket.Prefix == ArduinoMsgPrefix.ProbeConfigPrefix.Value) {
        if (msgPacket.Packet is ProbeControllerConfig config) {
            Console.WriteLine(JsonSerializer.Serialize(msgPacket,new JsonSerializerOptions() {
                Converters = {
                    new ArduinoMsgPrefixJsonConverter()
                },
                WriteIndented = true
            }));
        } else {
            Console.WriteLine("Failed to cast");
        }
    }
}

void RunUsbControllerTests() {
    UsbControllerTests tests = new UsbControllerTests();
    if (tests.Connect()) {
        tests.Run();
    }
}

IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder().ConfigureServices((_, services) => {
        var channel = Channel.CreateUnbounded<string>();
        services.AddSingleton(channel.Reader);
        services.AddSingleton(channel.Writer);
        services.AddTransient<UsbController>();
    });


/*async Task CreateStationDatabase(string stationId) {
    Console.WriteLine($"Creating Station Config Database.  Config for {stationId}");
    var client=new MongoClient("mongodb://172.20.3.41");
    StationDataService configService = new StationDataService(client);
    BurnStationConfiguration configuration = new BurnStationConfiguration();
    configuration.StationId = stationId;
    configuration.StationPosition = "P20";
    configuration.StationConfiguration = CreateStationConfig();
    configuration.ProbesConfiguration = CreateProbeControlConfig();
    configuration.HeaterConfig = CreateHeaterControlConfig();
    configuration._id = ObjectId.GenerateNewId();
    await configService.InsertConfiguration(configuration);
    Console.WriteLine("Check Database");
}*/

StationConfiguration CreateStationConfig() {
    var configuration =new StationConfiguration(1000, 500, 300000,3600000);
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

public class TestRef {
    public int Id { get; set; }
    public string Message { get; set; }
}




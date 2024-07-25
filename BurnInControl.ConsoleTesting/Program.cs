// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Text;
using System.Text.Json;
using BurnInControl.ConsoleTesting;
using BurnInControl.Data.StationModel;
using BurnInControl.Infrastructure.StationModel;
using MongoDB.Driver;
using BurnInControl.Data.BurnInTests;
using BurnInControl.Data.ComponentConfiguration;
using BurnInControl.Data.ComponentConfiguration.HeaterController;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.ComponentConfiguration.StationController;
using BurnInControl.Data.StationModel.Components;
using BurnInControl.Infrastructure.Dashboard;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Infrastructure.WaferTestLogs;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Packets;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.Shared.FirmwareData;
using MongoDB.Bson;
using Octokit;
using Octokit.Internal;
using QuickTest.Data.Models.Wafers.Enums;
using SerialPortLib;


//await TestGetReadings();
/*List<bool> checks = [true, true, true];
int count = 0;
var error=checks.Any(e=>!e) || count==0;
Console.WriteLine($"Error: {error}");*/
//await CreateDevStationDatabase();
//await TestParseWaferDataInitFinal();
//await TestGetWaferData();
//await CreateStationDatabase();
//await TestDashboardService();
//await GetTestLogs();

/*List<int> values = [1,2,3];

for(int i=0;i<=values.Count;i++) {
    if(i<values.Count && values.Count!=0) {
        Console.WriteLine(values[i]);
    } else {
        Console.WriteLine("End");
    }

}*/

TestListCompare();

void TestListCompare() {
    List<bool> l1 = [true, true, true];
    List<bool> l2 = [true, true, true];
    Console.WriteLine($"List Compare: {l1.SequenceEqual(l2)}");
}

async Task GetTestLogs() {
    string id = "669fab78323736492ca55665";
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    StationDataService stationDataService = new StationDataService(client);
    TestLogDataService testLogDataService=new TestLogDataService(client,stationDataService);
    var readings=await testLogDataService.GetTestLogReadings(ObjectId.Parse(id), StationPocket.LeftPocket);
    StringBuilder builder = new StringBuilder();
    foreach (var reading in readings) {
        builder.AppendLine($"{reading.Temperature}\t{reading.P1Current}\t{reading.P2Current}\t{reading.P1Voltage}\t{reading.P2Voltage}\t{reading.P1Runtime}\t{reading.P2Runtime}\t{reading.P2Runtime}");
    }
    File.WriteAllText(@"C:\Users\aelmendo\Documents\Test Logs\test.txt",builder.ToString());
}

async Task TestDashboardService() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    StationDataService stationDataService = new StationDataService(client);
    TestLogDataService testLogDataService=new TestLogDataService(client,stationDataService);
    WaferTestLogDataService waferTestLogDataService=new WaferTestLogDataService(client);
    DashboardTestService dashboardTestService=new DashboardTestService(testLogDataService,waferTestLogDataService);
    var readings=await dashboardTestService.GetWaferReadings("B02-3157-13");
    foreach (var reading in readings) {
        Console.WriteLine("Start Test");
        foreach (var data in reading) {
            Console.WriteLine(JsonSerializer.Serialize(data));
        }
    }
}


async Task TestGetWaferData() {
    List<string> _pads = [PadLocation.PadLocationA.Value,PadLocation.PadLocationB.Value,
        PadLocation.PadLocationC.Value, PadLocation.PadLocationR.Value,
        PadLocation.PadLocationL.Value,PadLocation.PadLocationT.Value,
        PadLocation.PadLocationG.Value];
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database=client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<WaferTestLog>("wafer_test_logs");
    var waferTestLog = await collection.Find(e => e.WaferId == "B04-1606-01").FirstOrDefaultAsync();
    if(waferTestLog is null) {
        Console.WriteLine("Not Found");
        return;
    }
    List<string> result=new List<string>();
    List<string> finalResult=new List<string>();
    List<string> pocketResult=new List<string>();
    foreach(var pad in _pads) {
        if (waferTestLog.WaferPadInitialData.ContainsKey(pad)) {
            result.Add(waferTestLog.WaferPadInitialData[pad].Voltage.ToString("F", CultureInfo.InvariantCulture));
            result.Add(waferTestLog.WaferPadInitialData[pad].Current.ToString("F", CultureInfo.InvariantCulture));
        } else {
            result.Add("0.00");
            result.Add("0.00");
        }
        if (waferTestLog.WaferPadFinalData.ContainsKey(pad)) {
            finalResult.Add(waferTestLog.WaferPadFinalData[pad].Voltage.ToString("F", CultureInfo.InvariantCulture));
            finalResult.Add(waferTestLog.WaferPadFinalData[pad].Current.ToString("F", CultureInfo.InvariantCulture));
        } else {
            finalResult.Add("0.00");
            finalResult.Add("0.00");
        }
            
        if (waferTestLog.PocketData.ContainsKey(pad)) {
            pocketResult.Add(waferTestLog.PocketData[pad].Pocket.ToString("D", CultureInfo.InvariantCulture));
            pocketResult.Add(waferTestLog.PocketData[pad].SetCurrent.ToString("D", CultureInfo.InvariantCulture));
            pocketResult.Add(waferTestLog.PocketData[pad].SetTemperature.ToString("D", CultureInfo.InvariantCulture));
        } else {
            pocketResult.Add("0");
            pocketResult.Add("0");
            pocketResult.Add("0");
        }
    }

    foreach (var data in result) {
        Console.WriteLine(data);
    }
    Console.WriteLine();
    Console.WriteLine();
    foreach (var data in finalResult) {
        Console.WriteLine(data);
    }
    Console.WriteLine();
    Console.WriteLine();
    foreach (var data in pocketResult) {
        Console.WriteLine(data);
    }
    Console.WriteLine();
    Console.WriteLine();

    Console.WriteLine("All Data:");
    result.AddRange(finalResult);
    result.AddRange(pocketResult);
    foreach (var data in result) {
        Console.WriteLine(data);
    }
    
}

async Task TestParseWaferDataInitFinal() {
    string logId = "667b06328853c7d2081ddd28";
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database=client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<BurnInTestLogEntry>("test_log_entries");
    var init=await collection.Find(e=>e.TestLogId==ObjectId.Parse(logId) && e.Reading.ElapsedSeconds>=10).FirstAsync();
    var final=await collection.Find(e=>e.TestLogId==ObjectId.Parse(logId) && e.Reading.ElapsedSeconds>=110).FirstAsync();
    Console.WriteLine($"Init: {init.Reading.ElapsedSeconds} Final: {final.Reading.ElapsedSeconds}");
}

async Task TestMongoDict() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database=client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<TestMongo>("test_dict");
    TestMongo test = new TestMongo();
    test._id=ObjectId.GenerateNewId();
    test.Value="Test";
    test.TestSetups = new Dictionary<string, TestMongoSetup>() {
        { StationPocket.LeftPocket.Name, new TestMongoSetup() { WaferId = "Wafer1", Data = 1 } },
        { StationPocket.MiddlePocket.Name, new TestMongoSetup() { WaferId = "Wafer2", Data = 2 } },
        { StationPocket.RightPocket.Name, new TestMongoSetup() { WaferId = "Wafer3", Data = 3 } }
    };
    await collection.InsertOneAsync(test);
    Console.WriteLine("Check Database");
}

async Task TestGetReadings() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database=client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<BurnInTestLogEntry>("test_log_entries");
    ObjectId id=ObjectId.Parse("6673380d0120cba9568813fc");
    var initFilter=Builders<BurnInTestLogEntry>.Filter.Eq(e => e.TestLogId, id) 
                   & Builders<BurnInTestLogEntry>.Filter.Gte(e => e.Reading.ElapsedSeconds, 5)
                   & Builders<BurnInTestLogEntry>.Filter.Lte(e => e.Reading.ElapsedSeconds, 10);
    var finalFilter = Builders<BurnInTestLogEntry>.Filter.Eq(e => e.TestLogId, id)
                      & Builders<BurnInTestLogEntry>.Filter.Gte(e => e.Reading.ElapsedSeconds, 20);
            
    var initTestLog = await collection.Find(initFilter)
        .Project(e => e.Reading)
        .FirstOrDefaultAsync();
    var finalTestLog = await collection.Find(finalFilter)
        .Project(e => e.Reading)
        .FirstOrDefaultAsync();
    if(initTestLog==null || finalTestLog==null) {
        Console.WriteLine("Logs Null");
        return;
    }
    Console.WriteLine($"Init: {initTestLog.ElapsedSeconds} Final: {finalTestLog.ElapsedSeconds}");
}

async Task CreateStationDatabase() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database=client.GetDatabase("burn_in_db");
    List<NetworkConfig> networkConfigs = [
        new NetworkConfig() { WifiIp = "172.20.5.150", EthernetIp = "172.20.5.160" },
        new NetworkConfig() { WifiIp = "172.20.5.151", EthernetIp = "172.20.5.161" },
        new NetworkConfig() { WifiIp = "172.20.5.152", EthernetIp = "172.20.5.162" },
        new NetworkConfig() { WifiIp = "172.20.5.153", EthernetIp = "172.20.5.163" },
        new NetworkConfig() { WifiIp = "172.20.5.154", EthernetIp = "172.20.5.164" },
        new NetworkConfig() { WifiIp = "172.20.5.155", EthernetIp = "172.20.5.165" },
        new NetworkConfig() { WifiIp = "172.20.5.156", EthernetIp = "172.20.5.166" },
        new NetworkConfig() { WifiIp = "172.20.5.157", EthernetIp = "172.20.5.167" },
        new NetworkConfig() { WifiIp = "172.20.5.158", EthernetIp = "172.20.5.168" },
        new NetworkConfig() { WifiIp = "172.20.5.159", EthernetIp = "172.20.5.169" }];

    List<HeaterNtc> heaterNtcValues = [
        new HeaterNtc(new NtcValues(0.00115864269299504, 0.000142930473584631, 1.11754468878939E-06),
            new NtcValues(0.00117301639823867, 0.000173625175643876, 7.35411421260852E-07),
            new NtcValues(0.00119989335654783, 0.000160361557296695, 8.50214455510871E-07)),
        new HeaterNtc(new NtcValues(0.0010403546818096,0.000195407080162592,6.61503783427737E-07),
            new NtcValues(0.00094001114756494,0.00021125166235281,5.93709983452035E-07),
            new NtcValues(0.000938545907702812,0.000214775563792224,5.49599295438701E-07)),
        new HeaterNtc(new NtcValues(0.000903903098729673,0.00020541621545903,7.19934029950399E-07),
            new NtcValues(0.00115802409151273,0.000181369373377678,6.65572940992084E-07), 
            new NtcValues(0.00121895370275201,0.000169750904102191,7.20352654377522E-07)),
        new HeaterNtc(new NtcValues(0.00090578537893032,0.000216847312412274,5.87102532634484E-07),
            new NtcValues(0.00105417599623743,0.000189192154765001,7.08523957926431E-07), 
            new NtcValues(0.00109374091173517,0.000192078010835684,6.22634180599287E-07)),
        new HeaterNtc(new NtcValues(0.00112392864926995,0.000171862897355418,8.38842044172239E-07),
            new NtcValues(0.00110174305250715,0.000182200632101591,7.31543630690517E-07), 
            new NtcValues(0.00120685034593358,0.000169572099316036,7.45541708352225E-07)),
        new HeaterNtc(new NtcValues(0.00082988588780555,0.000230121225749352,5.13319301512481E-07),
            new NtcValues(0.000922655273827692,0.000212941435680361,5.8644694087984E-07),
            new NtcValues(0.000950528210633288,0.000222049377989141,4.41930377865841E-07)),
        new HeaterNtc(new NtcValues(0.00108691723821753,0.000179518781531065,7.86904398529862E-07),
            new NtcValues(0.000817396344839526,0.000227136568390963,5.5811243066725E-07),
            new NtcValues(0.00104295985544091,0.000199997352351616,5.89027949056885E-07)),
        new HeaterNtc(new NtcValues(0.00138518005453178,0.000140344693098497,8.65415753821705E-07),
            new NtcValues(0.00109060697111335,0.00019267575646901,6.14906962977024E-07),
            new NtcValues(0.000612623871511618,0.000266099468163824,3.81084858516623E-07)),
        new HeaterNtc(new NtcValues(0.0009048314990115,0.000207341005493658,6.96780881482803E-07),
            new NtcValues(0.000678424348172734,0.00027373400501987,1.82473683435364E-07),
            new NtcValues(0.00165578046412743,9.13528903830797E-05,1.08960059707854E-06)),
        new HeaterNtc(new NtcValues(0.00132264118233491,0.000112787791062004,1.2846297515461E-06),
            new NtcValues(0.00134853900930964,0.000150911950453343,7.97334122501061E-07), 
            new NtcValues(0.00126818340314932,0.00014989501956629,9.00931599413797E-07)),
    ];
    for (int i = 0; i < 10; i++) {
        Station station = new Station();
        station.StationId=$"S0{i+1}";
        station.StationPosition=$"POS{i+1}";
        station.FirmwareVersion="V0.0.1";
        station.UpdateAvailable=false;
        station.State=StationState.Idle;
        station.RunningTest=null;
        station.SavedState=null;
        station.Configuration = new BurnStationConfiguration() {
            HeaterControllerConfig = new HeaterControllerConfig(),
            ProbeControllerConfig = new ProbeControllerConfig(),
            ControllerConfig = new StationConfiguration()
        };
        
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[0].NtcConfig.ACoeff = heaterNtcValues[i].H1.ACoeff;
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[0].NtcConfig.BCoeff = heaterNtcValues[i].H1.BCoeff;
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[0].NtcConfig.CCoeff = heaterNtcValues[i].H1.CCoeff;
        
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[1].NtcConfig.ACoeff = heaterNtcValues[i].H2.ACoeff;
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[1].NtcConfig.BCoeff = heaterNtcValues[i].H2.BCoeff;
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[1].NtcConfig.CCoeff = heaterNtcValues[i].H2.CCoeff;
        
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[2].NtcConfig.ACoeff = heaterNtcValues[i].H3.ACoeff;
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[2].NtcConfig.BCoeff = heaterNtcValues[i].H3.BCoeff;
        station.Configuration.HeaterControllerConfig.HeaterConfigurations[2].NtcConfig.CCoeff = heaterNtcValues[i].H3.CCoeff;
        
        station.NetworkConfig = networkConfigs[i];
        var collection=database.GetCollection<Station>("stations");
        await collection.InsertOneAsync(station);
    }
    Console.WriteLine("Check database");
}

async Task CreateDevStationDatabase() {
    Station station = new Station();
    station.StationId=$"S99";
    station.StationPosition=$"POS99";
    station.FirmwareVersion="V0.0.1";
    station.UpdateAvailable=false;
    station.State=StationState.Idle;
    station.RunningTest=null;
    station.SavedState=null;
    station.Configuration = new BurnStationConfiguration() {
        HeaterControllerConfig = new HeaterControllerConfig(),
        ProbeControllerConfig = new ProbeControllerConfig(),
        ControllerConfig = new StationConfiguration()
    };
    station.NetworkConfig= new NetworkConfig() { WifiIp = "0.0.0.0", EthernetIp = "0.0.0.0" };
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database=client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<Station>("stations");
    await collection.InsertOneAsync(station);
}

async Task CloneDatabase() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var piClient = new MongoClient("mongodb://192.168.68.111:27017");
    var database = client.GetDatabase("burn_in_db");
    var piDatabase = piClient.GetDatabase("burn_in_db");

    var stationCollection=database.GetCollection<Station>("stations");
    var piStationCollection=piDatabase.GetCollection<Station>("stations");

    var stations=await stationCollection.Find(Builders<Station>.Filter.Empty).ToListAsync();
    await piStationCollection.InsertManyAsync(stations);

    var trackerCollection=database.GetCollection<StationFirmwareTracker>("station_update_tracker");
    var piTrackerCollection=piDatabase.GetCollection<StationFirmwareTracker>("station_update_tracker");
    var trackers=await trackerCollection.Find(Builders<StationFirmwareTracker>.Filter.Empty).ToListAsync();
    await piTrackerCollection.InsertManyAsync(trackers);

    var versionCollection=database.GetCollection<VersionLog>("version_log");
    var piVersionCollection=piDatabase.GetCollection<VersionLog>("version_log");
    var versions=await versionCollection.Find(Builders<VersionLog>.Filter.Empty).ToListAsync();
    await piVersionCollection.InsertManyAsync(versions);

    var testConfigCollection=database.GetCollection<TestConfiguration>("test_configurations");
    var pitestConfigCollection=piDatabase.GetCollection<TestConfiguration>("test_configurations");
    var testConfigs=await testConfigCollection.Find(Builders<TestConfiguration>.Filter.Empty).ToListAsync();
    await pitestConfigCollection.InsertManyAsync(testConfigs);

    var logCollection=database.GetCollection<BurnInTestLog>("test_logs");
    var piLogCollection=piDatabase.GetCollection<BurnInTestLog>("test_logs");
    var logs = await logCollection.Find(_ => true).ToListAsync();
    await piLogCollection.InsertManyAsync(logs);
    
    var stateCollection=database.GetCollection<SavedStateLog>("saved_states");
    var piStateCollection=piDatabase.GetCollection<SavedStateLog>("saved_states");
    var states = await stateCollection.Find(_ => true).ToListAsync();
    await piStateCollection.InsertManyAsync(states);
    Console.WriteLine("Check Pi Database");

}

Task TestUsbController() {
    UsbTesting usb = new UsbTesting();
    Console.Clear();
    PrintMenu();
    var key=Console.ReadKey();
    while (key.Key != ConsoleKey.Escape) {
        switch (key.Key) {
            case ConsoleKey.D1:
                usb.Connect();
                break;
            case ConsoleKey.D2:
                usb.Send(StationMsgPrefix.CommandPrefix,StationCommand.ChangeModeATune);
                break;
            case ConsoleKey.D3:
                usb.Send(StationMsgPrefix.CommandPrefix, StationCommand.StartTune);
                break;
            case ConsoleKey.D4:
                usb.Send(StationMsgPrefix.CommandPrefix,StationCommand.StopTune);
                break;
            case ConsoleKey.D5:
                usb.Send(StationMsgPrefix.CommandPrefix,StationCommand.SaveTuning);
                break;
            case ConsoleKey.D6:
                usb.Send(StationMsgPrefix.CommandPrefix,StationCommand.SaveTuning);
                break;
            case ConsoleKey.D7:
                usb.Send(StationMsgPrefix.CommandPrefix,StationCommand.ChangeModeNormal);
                break;
            case ConsoleKey.D8:
                usb.Disconnect();
                break;
            default:
                break;
        }
        PrintMenu();
        key=Console.ReadKey();
    }
    return Task.CompletedTask;
}

void PrintMenu() {
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("1. Connect");
    Console.WriteLine("2. Change Mode AutoTune");
    Console.WriteLine("3. Start Tune");
    Console.WriteLine("4. Stop Tune");
    Console.WriteLine("5. Save Tune");
    Console.WriteLine("6. Cancel Tune");
    Console.WriteLine("7. Change Mode Normal");
    Console.WriteLine("7. Disconnect");
    Console.WriteLine();
}

async Task ConfigTestUsbController() {
    UsbTesting usb = new UsbTesting();
    IMongoClient client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<Station>("stations");
    var station=await collection.Find(e=>e.StationId=="S01").FirstOrDefaultAsync();
    if(station!=null) {
        Console.WriteLine("Station Found");
    } else {
        Console.WriteLine("Station Not Found");
        Console.WriteLine("Exiting...");
        return;
    }
    Console.Clear();
    PrintMenu();
    var key=Console.ReadKey();
    while (key.Key != ConsoleKey.Escape) {
        switch (key.Key) {
            case ConsoleKey.D1:
                usb.Connect();
                break;
            case ConsoleKey.D2:
                usb.Send(StationMsgPrefix.ReceiveConfigPrefix,
                    new ConfigPacket<HeaterControllerConfig>() { ConfigType = ConfigType.HeaterControlConfig, 
                        Configuration = station.Configuration.HeaterControllerConfig
                    });
                break;
            case ConsoleKey.D3:
                usb.Send(StationMsgPrefix.ReceiveConfigPrefix,
                    new ConfigPacket<ProbeControllerConfig>() {
                        ConfigType = ConfigType.ProbeControlConfig,
                        Configuration = station.Configuration.ProbeControllerConfig
                    });
                break;
            case ConsoleKey.D4:
                usb.Send(StationMsgPrefix.ReceiveConfigPrefix,
                    new ConfigPacket<StationConfiguration>() {
                        ConfigType = ConfigType.ControllerConfig,
                        Configuration = station.Configuration.ControllerConfig
                    });
                break;
            case ConsoleKey.D5:
                usb.Send(StationMsgPrefix.CommandPrefix,StationCommand.Reset);
                break;
            case ConsoleKey.D6:
                usb.Send(StationMsgPrefix.CommandPrefix,StationCommand.FormatSdCard);
                break;
            case ConsoleKey.D9:
                usb.Disconnect();
                break;
            default:
                break;
        }
        PrintMenuConfig();
        key=Console.ReadKey();
    }
}

void PrintMenuConfig() {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("1. Connect");
        Console.WriteLine("2. Send Heater");
        Console.WriteLine("3. Send Probe");
        Console.WriteLine("4. Send Station");
        Console.WriteLine("5. Get Heater");
        Console.WriteLine("6. Get Probe");
        Console.WriteLine("7. Get Station");
        Console.WriteLine("8. Disconnect");
        Console.WriteLine();
    }

 /*
 void StartSerialPort() {
     var serialPort= new SerialPortInput();
     serialPort.SetPort("COM3",38400);
     StringBuilder builder = new StringBuilder();
     serialPort.ConnectionStatusChanged += delegate(object sender, ConnectionStatusChangedEventArgs args)
     {
         Console.WriteLine("Connection Status: {0}", args.Connected);
     };
     serialPort.MessageReceived += delegate(object sender, MessageReceivedEventArgs args) {
         /*Console.WriteLine("Received message: {0}", BitConverter.ToString(args.Data));#1#
         var str=System.Text.Encoding.ASCII.GetString(args.Data);
         builder.Append(str);
         if (str.Contains('\n')) {
             Console.WriteLine($"Thread = {Thread.CurrentThread.ManagedThreadId} Received message: {builder.ToString()}");
             builder.Clear();
         }
        
     };

     if (serialPort.Connect()) {
         Console.WriteLine($"Connected {Thread.CurrentThread.ManagedThreadId}");
         PrintMenu();
         var key=Console.ReadKey();
         while (key.Key != ConsoleKey.Escape) {
             switch (key.Key) {
                 case ConsoleKey.D1:
                     MessagePacket<HeaterControllerConfig> msgPacket = new MessagePacket<HeaterControllerConfig>() {
                         Prefix = StationMsgPrefix.HeaterConfigPrefix,
                         Packet = new HeaterControllerConfig()
                     };
                     var output = JsonSerializer.Serialize(msgPacket,
                         new JsonSerializerOptions() {
                             PropertyNamingPolicy =null,
                             WriteIndented = false
                         });
                     serialPort.SendMessage(System.Text.Encoding.ASCII.GetBytes(output));
                     break;
                 case ConsoleKey.D2:
                     Console.WriteLine("Not Implemented");
                     break;
                 case ConsoleKey.D3:
                     Console.WriteLine("Not Implemented");
                     break;
                 case ConsoleKey.D4:
                     Console.WriteLine("Not Implemented");
                     break;
                 case ConsoleKey.D6:
                     serialPort.Disconnect();
                     break;
                 default:
                     break;
             }
             PrintMenu();
             key=Console.ReadKey();
         }
         serialPort.Disconnect();
         Console.WriteLine($"Goodbye {Thread.CurrentThread.ManagedThreadId}");
     } else {
         return;
     }


 }
 */

void PrintPackets() {
    MessagePacket<ConfigPacket<HeaterControllerConfig>> heaterConfigPacket = new MessagePacket<ConfigPacket<HeaterControllerConfig>>() {
        Prefix = StationMsgPrefix.ReceiveConfigPrefix, Packet = new ConfigPacket<HeaterControllerConfig>() {
            ConfigType = ConfigType.HeaterControlConfig, Configuration = new HeaterControllerConfig()
        }
    };
    /*MessagePacket<HeaterControllerConfig> heaterConfigPacket = new MessagePacket<HeaterControllerConfig>() {
        Prefix = StationMsgPrefix.HeaterConfigPrefix, Packet = new HeaterControllerConfig()
    };*/
    /*MessagePacket<ProbeControllerConfig> probeConfigPacket = new MessagePacket<ProbeControllerConfig>() {
        Prefix = StationMsgPrefix.ProbeConfigPrefix, Packet = new ProbeControllerConfig()
    };
    MessagePacket<StationConfiguration> stationConfigPacket = new MessagePacket<StationConfiguration>() {
        Prefix = StationMsgPrefix.StationConfigPrefix, Packet = new StationConfiguration()
    };*/
    Console.WriteLine();
    PrintPacketJson(heaterConfigPacket);
    Console.WriteLine();
    /*PrintPacketJson(probeConfigPacket);
    Console.WriteLine();
    PrintPacketJson(stationConfigPacket);
    Console.WriteLine();*/
}

void PrintPacketJson<TPacket>(MessagePacket<TPacket> msgPacket) where TPacket:IPacket {
    var output = JsonSerializer.Serialize(msgPacket,
        new JsonSerializerOptions() {
            PropertyNamingPolicy =null,
            WriteIndented = false
        });
    Console.WriteLine(output);
}



/*async Task BenchmarkLogFetching() {
    var stopWatch = new Stopwatch();
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<BurnInTestLog>("test_logs");
    var entryCollection=database.GetCollection<BurnInTestLogEntry>("test_log_entries");
    Console.WriteLine("Fetch Log");
    var id=ObjectId.Parse("662aca68480488cbab83ddf5");
    stopWatch.Start();
    var log=await collection.Find(e => e._id == id).FirstOrDefaultAsync();
    stopWatch.Stop();
    Console.WriteLine($"Fetch Log Time: {(double)stopWatch.ElapsedMilliseconds/1000} LogCount: {log.Readings.Count}");
    stopWatch.Reset();
    stopWatch.Start();
    var entries = await entryCollection.Find(e => e.TestLogId == id).ToListAsync();
    stopWatch.Stop();
    Console.WriteLine($"Entry Time: {(double)stopWatch.ElapsedMilliseconds/1000} EntryCount: {entries.Count}");
}*/




/*async Task TestLogsSepCollection() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<BurnInTestLog>("test_logs");
    var entryCollection=database.GetCollection<BurnInTestLogEntry>("test_log_entries");
    var logs = await collection.Find(_ => true).ToListAsync();
    List<BurnInTestLogEntry> entries = new List<BurnInTestLogEntry>();
    foreach (var log in logs) {
        foreach (var reading in log.Readings) {
            BurnInTestLogEntry entry = new BurnInTestLogEntry();
            foreach(var setup in log.TestSetup) {
                entry.WaferIds.Add(setup.WaferId ?? "none");
            }
            entry._id = ObjectId.GenerateNewId();
            entry.TestLogId = log._id;
            entry.Reading= reading;
            entries.Add(entry);
        }
        Console.WriteLine($"Inserting: Id:{log._id} Entries: {entries.Count}");
        await entryCollection.InsertManyAsync(entries);
        entries.Clear();
    }
}*/



async Task CopyTestLogs() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var piClient = new MongoClient("mongodb://192.168.68.112:27017");
    var database = client.GetDatabase("burn_in_db");
    var piDatabase = piClient.GetDatabase("burn_in_db");



    var logCollection=database.GetCollection<BurnInTestLog>("test_logs");
    var piLogCollection=piDatabase.GetCollection<BurnInTestLog>("test_logs");
    var logs = await piLogCollection.Find(_ => true).ToListAsync();
    foreach (var log in logs) {
        var other=await logCollection.Find(e => e._id == log._id).FirstOrDefaultAsync();
        if (other == null) {
            await logCollection.InsertOneAsync(log);
        }
    }

    Console.WriteLine("Done: Check Database");
}

async Task CreateTrackerDatabase() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("burn_in_db");
    var trackerCollection=database.GetCollection<StationFirmwareTracker>("station_update_tracker");
    var stationCollection=database.GetCollection<Station>("stations");
    var stations=await stationCollection.Find(Builders<Station>.Filter.Empty).ToListAsync();
    foreach(var station in stations) {
        StationFirmwareTracker tracker = new StationFirmwareTracker();
        tracker._id=ObjectId.GenerateNewId();
        tracker.StationId=station.StationId;
        tracker.CurrentVersion="V0.0.0";
        tracker.AvailableVersion="";
        tracker.UpdateAvailable = false;
        Console.WriteLine($"StationId {tracker.StationId}");
        await trackerCollection.InsertOneAsync(tracker);
    }
}

async Task UpdateFirmware() {
    var org = "Sensor-Electronic-Technology";
    var repo = "BurnInFirmware";
    var firmwarePath = "/source/ControlUpload/";
    var firmwareFileName = "BurnInFirmwareV3.ino.hex";
    var command = "";
    var program = "arduino-cli";
    var firmwareFullPath = firmwarePath + firmwareFileName;
}

async Task CheckLatest() {
    var org = "Sensor-Electronic-Technology";
    var repo = "BurnInFirmware";
    GitHubClient github = new GitHubClient(new ProductHeaderValue("Sensor-Electronic-Technology"));
    var release=await github.Repository.Release.GetLatest(org, repo);
    Console.WriteLine($"Release: {release.Name}");
}

async Task CreateRelease() {
    var org = "Sensor-Electronic-Technology";
    var repo = "BurnInFirmware";
    GitHubClient github = new GitHubClient(new ProductHeaderValue("Sensor-Electronic-Technology"),
        new InMemoryCredentialStore(new Credentials("aelmendorf","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")));


    /*var commit=(await github.Repository.Commit.GetAll(org, repo)).First();*/
    var refs = await github.Git.Reference.Get(org, repo, "heads/main");
    Console.WriteLine($"Sha: {refs.Object.Sha}");
    var commit=await github.Git.Commit.Get(org, repo, refs.Object.Sha);
    Console.WriteLine($"Sha: {commit.Sha}");
    Console.ReadKey();
    var tag = new NewTag {
        Message = "New Release tag",
        Tag = "V0.1.0",
        Type = TaggedType.Commit, // TODO: what are the defaults when nothing specified?
        Object = commit.Sha,
        Tagger = new Committer("aelmendorf","aelmendorf234@gmail.com",DateTimeOffset.Now)
    };
    
    var tadResult=await github.Git.Tag.Create(org,repo,tag);
    Console.WriteLine($"Tag: {tadResult.Tag}");
    var newRelease = new NewRelease(tag.Tag);
    newRelease.MakeLatest = MakeLatestQualifier.True;
    newRelease.Name = "Version: "+tag.Tag;
    newRelease.Body = "Release: "+tag.Tag;
    newRelease.Draft = false;
    newRelease.Prerelease = false;
    
    var result = await github.Repository.Release.Create(org, repo, newRelease);
    await using var archiveContents = File.OpenRead(@"C:\Users\aelmendo\Documents\Arduino\burn-build\BurnInFirmwareV3.ino.hex");
        
    var assetUpload = new ReleaseAssetUpload() 
    {
        FileName = "BurnInFirmwareV3.ino.hex",
        ContentType = "application/file",
        RawData = archiveContents
    };
    
    var release = await github.Repository.Release.Get(org,repo,result.Id);
    var asset = await github.Repository.Release.UploadAsset(release, assetUpload);
    Console.WriteLine($"Uploaded asset: {asset.Name}, {asset.Id}");
}

async Task GetVersionFromString() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("burn_in_db");
    var collection=database.GetCollection<VersionLog>("version_log");
    var latest = "V0.0.0";
    var split=latest.Split('.');
    var startidx=latest.IndexOf('V');
    var endidx=latest.IndexOf('.');
    var majorStr = latest.Substring(startidx + 1, (endidx - startidx) - 1);
    var minorstr = split[1];
    var patchstr = split[2];

    VersionLog versionLogEntry = new VersionLog();
    versionLogEntry._id= ObjectId.GenerateNewId();
    versionLogEntry.Version = latest;
    versionLogEntry.Major = int.Parse(majorStr);
    versionLogEntry.Minor = int.Parse(minorstr);
    versionLogEntry.Patch = int.Parse(patchstr);
    versionLogEntry.Latest = true;

    await collection.InsertOneAsync(versionLogEntry);
    Console.WriteLine("Check Database");

    Console.WriteLine($"Version: {versionLogEntry.Version}, Major: {versionLogEntry.Major}, Minor: {versionLogEntry.Minor}, Patch: {versionLogEntry.Patch}");

    foreach(var str in split) {
        Console.WriteLine(str);
    }
}

public class TestMongo {
    public ObjectId _id { get; set; }
    public string Value { get; set; }
    public Dictionary<string,TestMongoSetup> TestSetups { get; set; }
}

public class TestMongoSetup {
    public string WaferId { get; set; }
    public int Data { get; set; }
}

/*HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Add("Authorization", "Bearer mytoken");
//client.BaseAddress = new Uri("192.168.68.112:8080");
var response=await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,"http://192.168.68.112:8080/v1/update"));
if (response.StatusCode == HttpStatusCode.OK) {
    Console.WriteLine("Updates ran");
} else {
    Console.WriteLine("Updates Failed");
}*/


/*string text = "AutoTune";
bool sw=false;
sw = true;
for (int i = 0; i < 10; i++) {
    sw = !sw;
    text=sw ? "AutoTune":"Heating";
    Console.WriteLine(text);
}*/




/*MessagePacket<StationCommand> packet = new MessagePacket<StationCommand>() {
    Prefix = StationMsgPrefix.CommandPrefix,
    Packet = StationCommand.Reset
};*/
/*var jsonString=await File.ReadAllTextAsync(@"C:\temp\jsonTest.txt");
try {
    var doc=JsonSerializer.Deserialize<JsonDocument>(jsonString);
    var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
    var prefix=StationMsgPrefix.FromValue(prefixValue);
    Console.WriteLine($"Prefix: {prefix.Value}");
    var packetElem=doc.RootElement.GetProperty("Packet");

    var serialData = packetElem.Deserialize<StationSerialData>();
    Console.WriteLine($"Object: CurrentSetPoint: {serialData.CurrentSetPoint}");
}catch(Exception e) {
    Console.WriteLine(e.Message);
}*/

//var output=JsonSerializer.Serialize<MessagePacket<StationCommand>>(packet);
/*Console.WriteLine("Input: ");
Console.WriteLine(jsonString);*/


public record NtcValues(double ACoeff, double BCoeff, double CCoeff);

public record HeaterNtc(NtcValues H1, NtcValues H2, NtcValues H3);



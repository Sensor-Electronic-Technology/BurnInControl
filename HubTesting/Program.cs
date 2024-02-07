
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Models.StationData;
using HubTesting;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;



/*string path = @"C:\Users\aelmendo\Documents\test.json";
var writer = File.OpenWrite(path);

var testObject = new TestObject() {
    _id = 1,
    Name = "Object 1",
    Description = "A Test Object",
    Value = 54.678,
    Child = new TestObjectChild() {
        Number = 45,
        Key = "Child Key",
    }
};
JsonSerializer.SerializeAsync<TestObject>(writer,testObject,new JsonSerializerOptions(){PropertyNameCaseInsensitive = true});
Console.WriteLine($"Check File at {path}");*/
//await RunHubTest();
await RunControllerHubTests();
//SendHeaterConfig();


void SendHeaterConfig(bool newLine=false) {
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
    MessagePacket msgPacket = new MessagePacket {
        Prefix = ArduinoMsgPrefix.HeaterConfigPrefix,
        Packet = config
    };
    Console.WriteLine("MsgPacket");
    var msgJsonOut=JsonSerializer.Serialize(msgPacket, new JsonSerializerOptions() { WriteIndented = true });
    Console.WriteLine(msgJsonOut);
    Console.WriteLine("Config");
    var configJsonOut=JsonSerializer.Serialize(msgPacket, new JsonSerializerOptions() { WriteIndented = true });
    Console.WriteLine(configJsonOut);
}

async Task RunControllerHubTests() {
    var controllerTests=new ControllerHubTests();
    await controllerTests.Connect();
    await controllerTests.Run();
}

async Task RunHubTest() {
    var connection = new HubConnectionBuilder()
        .WithUrl(HubConstants.HubAddress)
        .Build();
    while (true) {
        try {
            await connection.StartAsync();
            Console.WriteLine("Connected");
            break;
        } catch {
            Thread.Sleep(500);
        }
    }
    
    connection.On<bool>(HubConstants.Events.OnUsbConnect, connected => {
        string status = connected ? "Connected":"Not Connected";
        Console.WriteLine($"Usb {status}");
    });
    
    connection.On<bool>(HubConstants.Events.OnExecuteCommand, success => {
        string status = success ? "Success":"Error Executing";
        Console.WriteLine($"Execute Command Status: {status}");
    });
    
    /*connection.On<RawReading>(HubConstants.Events.OnSerialCom, reading => {
        var output=JsonSerializer.Serialize<RawReading>(reading,
        new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(output);
    });*/

    connection.On<string>(HubConstants.Events.OnSerialComMessage, Console.WriteLine);
    
    var connected=await connection.InvokeAsync<bool>(HubConstants.Methods.ConnectUsb);
    string msg = connected ? "Usb Connected" : "Usb Connection Failed";
    Console.WriteLine(msg);
    if (connected) {
        Console.ReadLine();
        await connection.StopAsync();
    } else {
        await connection.StopAsync();
    }
    //Thread.Sleep(500);
}

Task WaitForStop(object obj,HubConnection connection) {
    Console.ReadKey();
    return connection.StopAsync();
}

public class TestObject {
    public int _id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double Value { get; set; }
    public TestObjectChild Child { get; set; }
}

public class TestObjectChild {
    public int Number { get; set; }
    public string Key { get; set; }
}
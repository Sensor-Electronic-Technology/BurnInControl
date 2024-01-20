using BurnIn.Shared;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models.BurnInStationData;
using HubServer;
using HubTesting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Threading.Channels;


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

async Task RunControllerHubTests() {
    var controllerTests=new ControllerHubTests();
    await controllerTests.Connect();
    controllerTests.Run();
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
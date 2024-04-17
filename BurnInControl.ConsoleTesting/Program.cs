// See https://aka.ms/new-console-template for more information

using System.Net;
using BurnInControl.ConsoleTesting.TestStateMachine;
using BurnInControl.ConsoleTesting.TestWorkflow;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.StationModel;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Net.Http;
using System.Text.Json;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using BurnInControl.Shared.ComDefinitions;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Services;

//await TestWorkFlow();

//TestStateMachine();

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Add("Authorization", "Bearer mytoken");
//client.BaseAddress = new Uri("192.168.68.112:8080");
var response=await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,"http://192.168.68.112:8080/v1/update"));
if (response.StatusCode == HttpStatusCode.OK) {
    Console.WriteLine("Updates ran");
} else {
    Console.WriteLine("Updates Failed");
}


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




void PrintMenu() {
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("1. Startup");
    Console.WriteLine("2. Connect");
    Console.WriteLine("3. Start");
    Console.WriteLine("4. Pause");
    Console.WriteLine("5. Continue");
    Console.WriteLine("6. Disconnect");
    Console.WriteLine("7. Exit");
    Console.WriteLine();
}

async Task TestWorkFlow() {
    /*var serviceProvider = ConfigureServices();
    var host=serviceProvider.GetService<IWorkflowHost>();*/
    //var workflowHost=new WorkflowHost();
    /*host.RegisterWorkflow<DataWorkflow,WorkflowData>();
    host.Start();
    host.StartWorkflow(nameof(DataWorkflow),new WorkflowData() {
        Message = "Message",
        Success = false
    });*/

}

static IServiceProvider ConfigureServices()
{
    //setup dependency injection
    IServiceCollection services = new ServiceCollection();
    //services.AddLogging();
    /*services.AddWorkflow();*/
    
    services.AddWorkflow(x => x.UseMongoDB(@"mongodb://172.20.3.41:27017", "burn_test_workflow"));
    var serviceProvider = services.BuildServiceProvider();

    return serviceProvider;
}


async Task CreateStationDatabase() {
    var client = new MongoClient("mongodb://192.168.68.112:27017");
    StationDataService stationService = new StationDataService(client);
    Station station = new Station();
}




// See https://aka.ms/new-console-template for more information

using BurnInControl.ConsoleTesting.TestStateMachine;
using BurnInControl.ConsoleTesting.TestWorkflow;
using BurnInControl.Data.ComponentConfiguration.ProbeController;
using BurnInControl.Data.StationModel;
using BurnInControl.Infrastructure.StationModel;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Text.Json;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Services;

//await TestWorkFlow();

TestStateMachine();


void TestStateMachine() {
    ControllerStateMachine stateMachine = new ControllerStateMachine();
    ConsoleKeyInfo key;
    while(true) {
        PrintMenu();
        key=Console.ReadKey();
        /*if(key.KeyChar=='1') {
            stateMachine.StartUp();
        } else if(key.KeyChar=='2') {
            stateMachine.Connect();
        } else if(key.KeyChar=='3') {
            stateMachine.Start();
        } else if(key.KeyChar=='4') {
            stateMachine.Pause();
        } else if(key.KeyChar=='5') {
            stateMachine.Continue();
        } else if(key.KeyChar=='6') {
            stateMachine.Disconnect();
        } else if(key.KeyChar=='7') {
            break;
        }*/
    }
    Console.WriteLine();

}

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
    services.AddLogging();
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




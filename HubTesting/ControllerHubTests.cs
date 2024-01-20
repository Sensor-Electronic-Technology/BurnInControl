using AsyncAwaitBestPractices;
using BurnIn.Shared.Controller;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text;
using System.Text.Json;
namespace HubTesting;

public class ControllerHubTests {
    private readonly HubConnection _connection;
    public ControllerHubTests() {
        this._connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5066/hubs/station")
            .Build();
    }

    public async Task Connect() {
        while (true) {
            try {
                await this._connection.StartAsync();
                Console.WriteLine("Connected");
                break;
            } catch {
                
                Thread.Sleep(500);
            }
        }
        this._connection.On<bool>(HubConstants.Events.OnUsbConnect, connected => {
            string status = connected ? "Connected":"Not Connected";
            Console.WriteLine($"Usb {status}");
        });

        this._connection.On<bool>(HubConstants.Events.OnExecuteCommand, success => {
            string status = success ? "Success" : "Error Executing";
            Console.WriteLine($"Execute Command Status: {status}");
        });
            
        this._connection.On<string>(HubConstants.Events.OnSerialComMessage, Console.WriteLine);
        var result=await this._connection.InvokeAsync<ControllerResult>(HubConstants.Methods.ConnectUsb);
        string msg = result.Success ? "Connected" : "Connection Failed";
        Console.WriteLine($"USB {msg}");
    }

    public async Task Disconnect() {
        Console.WriteLine("Disconnection");
        if (this._connection.State == HubConnectionState.Connected) {
            await this._connection.StopAsync();
        }
    }

    public void Run() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Select an option");
        builder.AppendLine("1: Send HeaterConfig");
        builder.AppendLine("2: Send ProbeConfig");
        builder.AppendLine("3: Send StationConfig");
        builder.AppendLine("4: Start");
        builder.AppendLine("5: Pause");
        builder.AppendLine("6: Reset");
        builder.AppendLine("7: Send Id(S02)");
        builder.AppendLine("8: Request Id");
        builder.AppendLine("9: Exit");
        Console.WriteLine(builder.ToString());
        while (true) {
            var key= Console.ReadKey().KeyChar;
            if (key == '0') {
                Console.Clear();
                Console.WriteLine(builder.ToString());
            }
            if (key == '1') {
                //this.SendHeaterConfig();
                Console.Clear(); 
            }else if (key == '2') {
                //this.SendProbeConfig();
                Console.Clear(); 
            }else if (key == '3') {
                //this.SendStationConfiguration();
                Console.Clear(); 
                Console.WriteLine($"Key= {key}");
            }else if (key == '4') {
                Console.Clear(); 
                this.SendCommand(ArduinoCommand.Start).SafeFireAndForget();
                Console.WriteLine($"Key= {key}");
            }else if (key == '5') {
                Console.Clear(); 
                this.SendCommand(ArduinoCommand.Pause).SafeFireAndForget();
                Console.WriteLine($"Key= {key}");
            }else if (key == '6') {
                Console.Clear();
                this.SendCommand(ArduinoCommand.Reset).SafeFireAndForget();
                Console.WriteLine($"Key= {key}");
            }else if (key == '7') {
                Console.Clear();
                //this.SendId();
            }else if (key == '8') {
                Console.Clear();
                //this.RequestId();
            }else if (key == '9') {
                //this._serialPortRx.Close();
                //this._cancellationTokenSource.Cancel();
                Console.WriteLine("Goodbye!!");
                break;
            }
        }
        
    }
    
    private async Task SendCommand(ArduinoCommand command,bool newLine = false) {
        var result = await this._connection.InvokeAsync<ControllerResult>(HubConstants.Methods.ExecuteCommand, command);
    }
    
}
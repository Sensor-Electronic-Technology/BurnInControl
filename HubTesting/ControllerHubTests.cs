using AsyncAwaitBestPractices;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Models.StationData;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text;
using System.Text.Json;
namespace HubTesting;

public class ControllerHubTests {
    private readonly HubConnection _connection;
    public ControllerHubTests() {
        this._connection = new HubConnectionBuilder()
            .WithUrl("http://172.20.1.15:3000/hubs/station")
            .Build();
        //.WithUrl("http://localhost:5066/hubs/station")
        //.WithUrl("http://192.168.68.112:3000/hubs/station")
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
        /*this._connection.On<bool>(HubConstants.Events.OnUsbConnect, connected => {
            string status = connected ? "Connected":"Not Connected";
            Console.WriteLine($"Usb {status}");
        });*/

        /*this._connection.On<bool>(HubConstants.Events.OnUsbDisconnect, disconnected => {
            if (disconnected) {
                Console.WriteLine("Usb Disconnected, please try reconnecting");
            } else {
                Console.WriteLine("Usb failed to disconnect, please reset service");
            }
        });*/

        this._connection.On<bool>(HubConstants.Events.OnExecuteCommand, success => {
            string status = success ? "Success" : "Error Executing";
            Console.WriteLine($"Execute Command Status: {status}");
        });

        this._connection.On<string>(HubConstants.Events.OnIdChanged, Console.WriteLine);

        this._connection.On<StationSerialData>(HubConstants.Events.OnSerialCom, this.HandleSerialData);
            
        this._connection.On<string>(HubConstants.Events.OnSerialComMessage, Console.WriteLine);

        this._connection.On<FirmwareUpdateStatus>(HubConstants.Events.OnUpdateChecked, status => {
            Console.WriteLine($"Ready?: {status.UpdateReady} Type: {nameof(status.Type)} Message: {status.Message}");
        });

        this._connection.On<string>(HubConstants.Events.OnReceiveFirmwareUploadText, message => {
            Console.WriteLine($"On Firmware Process Text: {message}");
        });

        this._connection.On<bool, string, string>(HubConstants.Events.OnFirmwareUpdated, this.OnUpdateCheckedHandler);
        
        await this._connection.InvokeAsync(HubConstants.Methods.ConnectUsb);
    }

    public async Task Disconnect() {
        Console.WriteLine("Disconnection");
        if (this._connection.State == HubConnectionState.Connected) {
            await this._connection.StopAsync();
        }
    }

    private void OnUpdateCheckedHandler(bool completed, string newVersion, string message) {
        Console.WriteLine($"Completed?: {completed} Version: {newVersion} Message: {message}");
    }

    private void HandleSerialData(StationSerialData serialData) {
        StringBuilder builder = new StringBuilder();

        builder.AppendFormat($"Elapsed: {serialData.ElapsedSeconds} ");
        
        builder.Append("Voltages: ");
        for (int i = 0; i < 6; i++) {
            builder.AppendFormat($" V[{i}] {serialData.Voltages[i]} ");
        }
        
        builder.Append(" Currents: ");
        for (int i = 0; i < 6; i++) {
            builder.AppendFormat($" I[{i}] {serialData.Currents[i]} ");
        }
        
        builder.Append(" Temps: ");
        for (int i = 0; i < 3; i++) {
            builder.AppendFormat($" T[{i}] {serialData.Temperatures[i]} ");
        }
        Console.WriteLine(builder.ToString());
    }

    public async Task Run() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Select an option");
        /*builder.AppendLine("1: Send HeaterConfig");
        builder.AppendLine("2: Send ProbeConfig");
        builder.AppendLine("3: Send StationConfig");*/
        builder.AppendLine("1: Send Version");
        builder.AppendLine("2: Check Updates");
        builder.AppendLine("3: Update");
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
                this._connection.InvokeAsync(HubConstants.Methods.ConnectUsb).SafeFireAndForget();
            }
            /*if (key == '1') {
                await this.SendHeaterConfig();
                Console.Clear(); 
            }else if (key == '2') {
                await this.SendProbeConfig();
                Console.Clear(); 
            }else if (key == '3') {
                await this.SendStationConfiguration();
                Console.Clear(); 
                Console.WriteLine($"Key= {key}");*/
            if (key == '1') {
                await this.SendFirmwareVersion();
                Console.Clear(); 
            }else if (key == '2') {
                await this.SendCheckUpdate();
                Console.Clear(); 
            }else if (key == '3') {
                this.UpdateFirmware().SafeFireAndForget();
                Console.Clear(); 
                Console.WriteLine($"Key= {key}");
            }else if (key == '4') {
                Console.Clear();
                await this.SendCommand(ArduinoCommand.Start);
                Console.WriteLine($"Key= {key}");
            }else if (key == '5') {
                Console.Clear();
                await this.SendCommand(ArduinoCommand.Pause);
                Console.WriteLine($"Key= {key}");
            }else if (key == '6') {
                Console.Clear();
                await this.SendCommand(ArduinoCommand.Reset);
                Console.WriteLine($"Key= {key}");
            }else if (key == '7') {
                Console.Clear();
                await this.SendId();
            }else if (key == '8') {
                Console.Clear();
                await this.RequestId();
            }else if (key == '9') {
                await this._connection.StopAsync();
                Console.WriteLine("Goodbye!!");
                break;
            }
        }
        
    }
    
    private async Task SendCommand(ArduinoCommand command) {
        await this._connection.InvokeAsync(HubConstants.Methods.SendCommand, command);
    }
    
    private async Task SendProbeConfig() {
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
        await this._connection.InvokeAsync(HubConstants.Methods.SendProbeConfig, probeControllerConfig);
    }

    private async Task SendCheckUpdate() {
        await this._connection.InvokeAsync(HubConstants.Methods.CheckForUpdate);
    }

    private async Task SendFirmwareVersion() {
        await this._connection.InvokeAsync(HubConstants.Methods.SendFirmwareVersion, "V1.0.0");
    }

    private async Task UpdateFirmware() {
        await this._connection.InvokeAsync(HubConstants.Methods.UpdateFirmware);
    }
    
    private async Task SendHeaterConfig() {
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
        await this._connection.InvokeAsync(HubConstants.Methods.SendHeaterConfig, config);
    }
    
    private async Task SendStationConfiguration(bool newLine=false) {
        var configuration =new StationConfiguration(1000, 500, 300000,3600000);
        var burnTimerConfig = new BurnTimerConfig(72000, 72000, 25200);
        configuration.BurnTimerConfig = burnTimerConfig;
        await this._connection.InvokeAsync(HubConstants.Methods.SendStationConfig,configuration);
    }
    
    private async Task SendId(bool newLine=false) {
        await this._connection.InvokeAsync(HubConstants.Methods.SendId, "S11");
    }
    
    private async Task RequestId() {
        await this._connection.InvokeAsync(HubConstants.Methods.RequestId);
    }
    
    private async Task SendVersion(bool newLine=false) {
        await this._connection.InvokeAsync(HubConstants.Methods.SendId, "S11");
    }
    
    private async Task RequestVersion() {
        await this._connection.InvokeAsync(HubConstants.Methods.RequestId);
    }
    
}
using AsyncAwaitBestPractices;
using BurnIn.ControlService.Services;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using CP.IO.Ports;
using ReactiveMarbles.Extensions;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using StationConfiguration=BurnIn.Shared.Models.Configurations.StationConfiguration;
namespace BurnIn.UsbTesting;

public class UsbControllerTests {
    private readonly UsbController _usbController;
    private ChannelReader<string> _channelReader;
    private Channel<string> _channel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public UsbControllerTests() {
        this._channel = Channel.CreateUnbounded<string>();
        this._usbController = new UsbController(this._channel.Writer);
        this._channelReader = this._channel.Reader;
    }

    public bool Connect() {
        return this._usbController.Connect().IsSuccess;
    }

    public void Run() {
        this.Reader(this._cancellationTokenSource.Token).SafeFireAndForget(e => {
            Console.WriteLine($"Read Error: {e.Message}");
        });
        
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
                Console.WriteLine("Reconnecting");
                var result=this._usbController.Connect();
                if (result.IsSuccess) {
                    Console.WriteLine("Usb Connected Successfully");
                    Console.WriteLine(builder.ToString());
                } else {
                    Console.WriteLine("Usb failed to connect");
                    break;
                }
            }
            if (key == '1') {
                this.SendHeaterConfig();
                Console.Clear(); 
            }else if (key == '2') {
                this.SendProbeConfig();
                Console.Clear(); 
            }else if (key == '3') {
                this.SendStationConfiguration();
                Console.Clear(); 
                Console.WriteLine($"Key= {key}");
            }else if (key == '4') {
                Console.Clear(); 
                this.SendCommand(ArduinoCommand.Start);
                Console.WriteLine($"Key= {key}");
            }else if (key == '5') {
                Console.Clear(); 
                this.SendCommand(ArduinoCommand.Pause);
                Console.WriteLine($"Key= {key}");
            }else if (key == '6') {
                Console.Clear();
                this.SendCommand(ArduinoCommand.Reset);
                Console.WriteLine($"Key= {key}");
            }else if (key == '7') {
                Console.Clear();
                this.SendId();
            }else if (key == '8') {
                Console.Clear();
                this.RequestId();
            }else if (key == '9') {
                //this._serialPortRx.Close();
                this._usbController.Disconnect();
                this._cancellationTokenSource.Cancel();
                Console.WriteLine("Goodbye!!");
                break;
            }
        }
       
    }

    private async Task Reader(CancellationToken token) {
        await foreach (var message in this._channelReader.ReadAllAsync(token)) {
            Console.WriteLine(message);
        }
    }
    
    private void SendCommand(ArduinoCommand command,bool newLine = false) {
        MessagePacketV2<ArduinoCommand> msgPacket = new MessagePacketV2<ArduinoCommand>() {
            Prefix = ArduinoMsgPrefix.CommandPrefix,
            Packet = command
        };
        var output = JsonSerializer.Serialize<MessagePacketV2<ArduinoCommand>>(msgPacket,
                new JsonSerializerOptions(){WriteIndented = false});
        this._usbController.Send(msgPacket);
    }

    private void SendId(bool newLine=false) {
        MessagePacketV2<StationIdPacket> msgPacket = new MessagePacketV2<StationIdPacket>() {
            Prefix = ArduinoMsgPrefix.IdReceive,
            Packet = new StationIdPacket(){StationId = "S22"}
        };
        var output = JsonSerializer.Serialize<MessagePacketV2<StationIdPacket>>(msgPacket,
        new JsonSerializerOptions(){WriteIndented = false});
        this._usbController.Send(msgPacket);
    }
    
    private void RequestId(bool newLine=false) {
        MessagePacketV2<ArduinoMsgPrefix> msgPacket =new MessagePacketV2<ArduinoMsgPrefix>(){
            Prefix = ArduinoMsgPrefix.IdRequest,
            Packet = ArduinoMsgPrefix.IdRequest
        };
        /*var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions(){WriteIndented = false});*/
        this._usbController.Send(msgPacket);
    }
    
    private void SendProbeConfig(bool newLine=false) {
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
        MessagePacketV2<ProbeControllerConfig> msgPacket = new MessagePacketV2<ProbeControllerConfig>() {
            Prefix = ArduinoMsgPrefix.ProbeConfigPrefix,
            Packet = probeControllerConfig
        };
        /*var output=JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions { WriteIndented = false });*/
        this._usbController.Send(msgPacket);
    }
    
    public void SaveProbeConfigFile() {
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
        MessagePacket msg = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.ProbeConfigPrefix,
            Packet = probeControllerConfig
        };
        var output=JsonSerializer.Serialize<MessagePacket>(msg,
        new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\ProbeConfigJson.txt",output);
    }
    
    private void SendHeaterConfig(bool newLine=false) {
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
        MessagePacketV2<HeaterControllerConfig> msgPacket = new MessagePacketV2<HeaterControllerConfig>() {
            Prefix = ArduinoMsgPrefix.HeaterConfigPrefix,
            Packet = config
        };
        /*var output=JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions { WriteIndented = false });*/
        this._usbController.Send(msgPacket);
    }

    private void SendStationConfiguration(bool newLine=false) {
        var configuration =new StationConfiguration(1000,500,300000,3600000);
        var burnTimerConfig = new BurnTimerConfig(72000, 72000, 25200);
        configuration.BurnTimerConfig = burnTimerConfig;
        MessagePacketV2<StationConfiguration> msgPacket = new MessagePacketV2<StationConfiguration> {
            Prefix = ArduinoMsgPrefix.StationConfigPrefix,
            Packet = configuration
        };
        this._usbController.Send(msgPacket);
    }
}
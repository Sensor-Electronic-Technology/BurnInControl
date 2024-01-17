using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using System.IO.Ports;
using System.Text;
using System.Text.Json;
using StationConfiguration=BurnIn.Shared.Models.Configurations.StationConfiguration;

namespace BurnIn.UsbTesting;

public class ControllerTests {
    private SerialPort _serialPort=new SerialPort("COM3",38400);
    
    public void Setup() {
        this._serialPort.DataReceived += SerialDataReceived;
    }

    public void Connect() {
        try {
            this._serialPort.Open();
        } catch {
            Console.WriteLine("Serial port failed to connect");
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
        builder.AppendLine("7: Exit");

        while (true) {
            Console.WriteLine(builder.ToString());
            var key= Console.ReadKey().KeyChar;
            if (key == '1') {
                this.SendHeaterConfig();
                Console.Clear(); 
            }else if (key == '2') {
                this.SendProbeConfig();
                Console.Clear(); 
            }else if (key == '3') {
                Console.WriteLine($"Key= {key}");
                this.SendStationConfiguration();
                Console.Clear(); 
            }else if (key == '4') {
                this.SendCommand(ArduinoCommand.Start);
                Console.Clear(); 
            }else if (key == '5') {
                this.SendCommand(ArduinoCommand.Pause);
                Console.Clear(); 
            }else if (key == '6') {
                this.SendCommand(ArduinoCommand.Reset);
                Console.Clear();   
            }else if (key == '7') {
                this._serialPort.Close();
                Console.WriteLine("Goodbye!!");
                break;
            }
            
        }
    }

    private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e) {
        var serialPort = (SerialPort)sender;
        if (serialPort.BytesToRead > 1) {
            var data=serialPort.ReadLine();
            Console.WriteLine(data);
            /*var doc=JsonSerializer.Deserialize<JsonDocument>(data);
            var prefix=doc.RootElement.GetProperty("Prefix").ToString();*/
            /*if (!string.IsNullOrEmpty(prefix)) {
                if (prefix == ArduinoMsgPrefix.ProbeConfigPrefix.Value) {
                    var received = doc.RootElement.GetProperty("Packet").Deserialize<ProbeControllerConfig>();
                    var output=JsonSerializer.Serialize(received, new JsonSerializerOptions(
                    new JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine("Success!! See Probe Config Json Below ");
                    Console.WriteLine(output);
                }else if (prefix == ArduinoMsgPrefix.HeaterConfigPrefix.Value) {
                    var received = doc.RootElement.GetProperty("Packet").Deserialize<HeaterControllerConfiguration>();
                    var output=JsonSerializer.Serialize(received, new JsonSerializerOptions(
                    new JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine("Success!! See Heater Config Json Below ");
                    Console.WriteLine(output);
                }else if (prefix == ArduinoMsgPrefix.StationConfigPrefix.Value) {
                    //var received = doc.RootElement.GetProperty("Packet").Deserialize<ProbeControllerConfig>();
                    Console.WriteLine("Error: Station configuration not implemented yet");
                }else if (prefix == ArduinoMsgPrefix.MessagePrefix.Value) {

                }else if (prefix == ArduinoMsgPrefix.DataPrefix.Value) {

                }else {
                    Console.WriteLine("Error: Deserialization Failed. Invalid prefix");
                    Console.WriteLine(data);
                }
            } else {
                Console.WriteLine("Error: Prefix missing,check input below");
                Console.WriteLine(data);
            }*/
        }
    }

    private void SendCommand(ArduinoCommand command,bool newLine = false) {
        MessagePacket msgPacket = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.CommandPrefix.Value,
            Packet = command.Value
        };
        var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
                new JsonSerializerOptions(){WriteIndented = false});
        if (newLine) {
            this._serialPort.WriteLine(output);
        } else {
            this._serialPort.Write(output);
        }
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
        MessagePacket msg = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.ProbeConfigPrefix.Value,
            Packet = probeControllerConfig
        };
        var output=JsonSerializer.Serialize<MessagePacket>(msg,
        new JsonSerializerOptions { WriteIndented = false });
        if (newLine) {
            this._serialPort.WriteLine(output);
        } else {
            this._serialPort.Write(output);
        }
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

        HeaterControllerConfiguration configuration = new HeaterControllerConfiguration();
        configuration.HeaterConfigurations = [
            heaterConfig1,
            heaterConfig1,
            heaterConfig2
        ];
        MessagePacket msgPacket = new MessagePacket {
            Prefix = ArduinoMsgPrefix.HeaterConfigPrefix.Value,
            Packet = configuration
        };
        var output=JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions { WriteIndented = false });
        if (newLine) {
            this._serialPort.WriteLine(output);
        } else {
            this._serialPort.Write(output);
        }
        
    }

    private void SendStationConfiguration(bool newLine=false) {
        var configuration =new StationConfiguration(1000, 500, 60000);
        var burnTimerConfig = new BurnTimerConfig(72000, 72000, 25200);
        configuration.BurnTimerConfig = burnTimerConfig;
        MessagePacket msgPacket = new MessagePacket {
            Prefix = ArduinoMsgPrefix.StationConfigPrefix.Value,
            Packet = configuration
        };
        var output=JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions { WriteIndented = false });
        if (newLine) {
            this._serialPort.WriteLine(output);
        } else {
            this._serialPort.Write(output);
        }
        
    }
}
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
using StationConfiguration=BurnIn.Shared.Models.Configurations.StationConfiguration;

namespace BurnIn.UsbTesting;

public class ControllerTests {
    private readonly SerialPortRx _serialPortRx=new SerialPortRx("COM3",38400);
    private readonly SerialPort _serialPort = new SerialPort("COM3", 38400);
    private ConcurrentQueue<string> _bufferQueue=new ConcurrentQueue<string>();
    private bool _continue=false;
    readonly CompositeDisposable _comdis = new CompositeDisposable();
    private Thread _readThread;
    private Thread _handleThread;
    private object _lock = new object();
    ManualResetEvent _readyEvent = new ManualResetEvent(false);

    public ControllerTests(){
        this._readThread= new Thread(SerialPortDataThread);
        this._handleThread = new Thread(HandleThread);
    }
    
    public void Setup() {
        //this._serialPort.DataReceived += SerialDataReceived;
        /*var startChar = Convert.ToInt32('{').AsObservable();
        var endChar = Convert.ToInt32('\n').AsObservable();*/
        /*this._serialPortRx.DataReceived
            .BufferUntil(startChar,endChar,100)
            .Subscribe(this.DataReceivedHandler)
            .DisposeWith(this._comdis);
        this._serialPortRx.ErrorReceived
            .Subscribe(Console.WriteLine)
            .DisposeWith(this._comdis);*/
    }

    public bool Connect() {
        try {
            //this._serialPortRx.Open();
            this._serialPort.Open();
            return true;
        } catch {
            Console.WriteLine("Serial port failed to connect");
            return false;
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
        this._continue = true;
        this._readThread.Start();
        this._handleThread.Start();

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
                Console.WriteLine("Goodbye!!");
                break;
            }
            
        }
        this._continue = false;
        this._readThread.Join();
        this._handleThread.Join();
        this._serialPort.Close();
    }

    public void SerialPortDataThread() {
        while (this._continue) {
            string message=string.Empty;
            if (Monitor.TryEnter(this._serialPort, 500)) {
                try {
                    message=_serialPort.ReadLine();
                } catch(TimeoutException) { } finally {
                    Monitor.Exit(this._serialPort);
                }
            }
            if (!string.IsNullOrEmpty(message)) {
                var startIndex=message.IndexOf('{');
                if (startIndex >= 0) {
                    var input=message.Substring(startIndex, message.Length-startIndex);
                    //Console.WriteLine(input);
                    if (Monitor.TryEnter(this._bufferQueue)) {
                        try {
                            this._bufferQueue.Enqueue(input);
                            this._readyEvent.Set();
                        } finally {
                            Monitor.Exit(this._bufferQueue);
                        }
                    }
                }
            }
        }
    }

    public void HandleThread() {
        while (this._continue) {
            this._readyEvent.WaitOne();
            if (this._bufferQueue.TryDequeue(out var message)) {
                Console.WriteLine(message);   
            }
        }
    }

    private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e) {
        var serialPort = (SerialPort)sender;
        if (serialPort.BytesToRead > 1) {
            var data=serialPort.ReadLine();
            //Console.WriteLine(data);
            try {
                var doc = JsonSerializer.Deserialize<JsonDocument>(data);
                var prefix = doc.RootElement.GetProperty("Prefix").ToString();
                
                if (prefix == ArduinoMsgPrefix.MessagePrefix.Value) {
                    var received = doc.RootElement.GetProperty("Packet").Deserialize<JsonDocument>();
                    var message = received.RootElement.GetProperty("Message").Deserialize<string>();
                    Console.WriteLine($"Arduino Message: {message}");
                }else if (prefix == ArduinoMsgPrefix.ProbeConfigPrefix.Value) {
                    var received = doc.RootElement.GetProperty("Packet").Deserialize<ProbeControllerConfig>();
                    var output=JsonSerializer.Serialize(received, new JsonSerializerOptions(
                    new JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine("Success!! See Probe Config Json Below ");
                    Console.WriteLine(output);
                }
            } catch {
                Console.WriteLine("Deserialization Failed");
            }
        }
    }
    
    private void DataReceivedHandler(string data) {
        try {
            var doc = JsonSerializer.Deserialize<JsonDocument>(data);
            var prefix = doc.RootElement.GetProperty("Prefix").ToString();
            if (prefix == ArduinoMsgPrefix.MessagePrefix.Value) {
                var received = doc.RootElement.GetProperty("Packet").Deserialize<JsonDocument>();
                var message = received.RootElement.GetProperty("Message").Deserialize<string>();
                Console.WriteLine($"Arduino Message: {message}");
            }else if (prefix == ArduinoMsgPrefix.ProbeConfigPrefix.Value) {
                var received = doc.RootElement.GetProperty("Packet").Deserialize<ProbeControllerConfig>();
                var output=JsonSerializer.Serialize(received, new JsonSerializerOptions(
                new JsonSerializerOptions { WriteIndented = false }));
                Console.WriteLine("Success!! See Probe Config Json Below ");
                Console.WriteLine(output);
            }else if (prefix == ArduinoMsgPrefix.HeaterConfigPrefix.Value) {
                var received = doc.RootElement.GetProperty("Packet").Deserialize<HeaterControllerConfig>();
                var output=JsonSerializer.Serialize(received, new JsonSerializerOptions(
                new JsonSerializerOptions { WriteIndented = false }));
                Console.WriteLine("Success!! See Heater Config Json Below ");
                Console.WriteLine(output);
            }else if (prefix == ArduinoMsgPrefix.StationConfigPrefix.Value) {
                var received = doc.RootElement.GetProperty("Packet").Deserialize<StationConfiguration>();
                var output=JsonSerializer.Serialize(received, new JsonSerializerOptions(
                new JsonSerializerOptions { WriteIndented = false }));
                Console.WriteLine("Success!! See Station Config Json Below ");
                Console.WriteLine(output);
            }
        } catch {
            Console.WriteLine("Deserialization Failed,data below");
            Console.WriteLine(data);
        }
    }

    private void SendCommand(ArduinoCommand command,bool newLine = false) {
        MessagePacket msgPacket = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.CommandPrefix.Value,
            Packet = command.Value
        };
        var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
                new JsonSerializerOptions(){WriteIndented = false});
        lock (this._lock) {
            if (newLine) {
                this._serialPort.WriteLine(output);
            } else {
                this._serialPort.Write(output);
            }
        }
        Console.WriteLine($"Sent: {output}");
    }

    private void SendId(bool newLine=false) {
        MessagePacket msgPacket = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.IdReceive.Value,
            Packet = "S09"
        };
        var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions(){WriteIndented = false});
        lock (this._lock) {
            if (newLine) {
                this._serialPort.WriteLine(output);
            } else {
                this._serialPort.Write(output);
            }
        }
    }
    
    private void RequestId(bool newLine=false) {
        MessagePacket msgPacket = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.IdRequest,
            Packet = "S02"
        };
        var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions(){WriteIndented = false});
        lock (this._lock) {
            if (newLine) {
                this._serialPort.WriteLine(output);
            } else {
                this._serialPort.Write(output);
            }
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
            this._serialPortRx.WriteLine(output);
        } else {
            this._serialPortRx.Write(output);
        }
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
            Prefix = ArduinoMsgPrefix.ProbeConfigPrefix.Value,
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
        MessagePacket msgPacket = new MessagePacket {
            Prefix = ArduinoMsgPrefix.HeaterConfigPrefix.Value,
            Packet = config
        };
        var output=JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions { WriteIndented = false });
        if (newLine) {
            this._serialPortRx.WriteLine(output);
        } else {
            this._serialPortRx.Write(output);
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
            this._serialPortRx.WriteLine(output);
        } else {
            this._serialPortRx.Write(output);
        }
    }
}
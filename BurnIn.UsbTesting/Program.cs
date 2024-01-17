// See https://aka.ms/new-console-template for more information
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using CP.IO.Ports;
using ReactiveMarbles.Extensions;
using System.Collections.Concurrent;

using System.Diagnostics;
using System.IO.Ports;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Controller;
using BurnIn.UsbTesting;
using ConsoleTools;
using DataReceivedEventArgs=System.Diagnostics.DataReceivedEventArgs;

ControllerTests tests = new ControllerTests();
tests.Setup();
tests.Connect();
tests.Run();




async Task TestUsbController() {
    UsbController usb = new UsbController();
    usb.SerialDataReceived += DataReceivedHandler;
    var response = usb.Connect("COM3");
    if (response.State!=UsbState.Connected) {
        Console.WriteLine(response.Message);
        return;
    }
    Console.WriteLine("Usb Connected. See menu below");
    CreateAndOutputHeaterConfig(usb);
    
    CreateAndOutputProbeConfig(usb);
    while (true) {
        Console.ReadLine();
        break;
    }
    usb.Disconnect();
}

async void DataReceivedHandler(object? sender,UsbDataReceivedEventArgs args) {
    Console.WriteLine("Data Received");
    Console.WriteLine(args.Data);
}



string FindPort() {
    Process process = new Process();
    string fileName =@"/home/setiburnin/Documents/test.sh";
    ProcessStartInfo startInfo = new ProcessStartInfo { 
        FileName = fileName,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };
    process.StartInfo = startInfo;
    process.Start();
    var result = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    var lines=result.Split('\n');
    Console.WriteLine($"Lines: {lines.Length}");
    var arduino=lines.FirstOrDefault(e => e.Contains("Arduino"));
    if (!string.IsNullOrEmpty(arduino)) {
        Console.WriteLine($"Found Arduino: ");
        int index=arduino.IndexOf('-');
        if (index >= 0) {
            var portName=arduino.Substring(0, index - 1);
            Console.WriteLine($"Port Name: {portName.Trim()}");
            return portName.Trim();
        } else {
            Console.WriteLine("Error Parsing Port Name");
            return string.Empty;
        }
    } else {
        Console.WriteLine("Error: USB Not Found");
        return string.Empty;
    }
}

void StartSerialRx() {
    //var comPortName = FindPort();
    /*var startChar = 0x21.AsObservable();*/
    //var endChar = 0x0D.AsObservable();

    var startChar = Convert.ToInt32('{').AsObservable();
    var endChar = Convert.ToInt32('\n').AsObservable();
    
    var dis = new CompositeDisposable();
    var comdis = new CompositeDisposable();
    
    var port=new SerialPortRx("COM3", 38400);
    port.DisposeWith(comdis);
    port.ErrorReceived.Subscribe(Console.WriteLine).DisposeWith(comdis);
    //port.IsOpenObservable.Subscribe(x => Console.WriteLine($"Port {comPortName} is {(x ? "Open" : "Closed")}")).DisposeWith(comdis);
    port.DataReceived.BufferUntil(startChar, endChar, 100).Subscribe(HandlerSerialConfig).DisposeWith(comdis);
    //port.DataReceived.Subscribe(data => Console.WriteLine(data)).DisposeWith(comdis);
    
    port.Open();
    Console.WriteLine("Press q to quite, anything else to send");
    while (true) {
        var key=Console.ReadKey();
        if (key.KeyChar == 'q') {
            break;
        } else {
            //string output = "x";
            var output=CreateAndOutputProbeConfigOther();
            Console.WriteLine(output);
            port.Write(output);
        }

        //port.WriteLine(CreateData());
        //Thread.Sleep(2000);
    }
    //Console.ReadLine();
    //comdis.Dispose();
    //dis.Dispose();
}

string CreateData() {
    ArduinoSerialData serialData = new ArduinoSerialData();
    for (int i = 0; i < 6; i++) {
        serialData.Currents.Add(150);
        serialData.Voltages.Add(65);
        serialData.ProbeRuntimes.Add(32000);
    }
    serialData.Temperatures.Add(85);
    serialData.Temperatures.Add(85);
    serialData.Temperatures.Add(85);

    serialData.Heater1State = true;
    serialData.Heater2State = false;
    serialData.Heater3State = true;

    serialData.RuntimeSeconds=560789;

    var output=JsonSerializer.Serialize<ArduinoSerialData>(serialData,
    new JsonSerializerOptions { WriteIndented = false });
    return output;
    //await File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\SerialData.txt",output);
}

void HandleSerial(string data) {
    var aData=JsonSerializer.Deserialize<ArduinoSerialData>(data);
    foreach (var runtTime in aData.ProbeRuntimes) {
        Console.Write($"{runtTime} ,");
    }
    Console.WriteLine();
    //Console.WriteLine(data);
}

void HandlerSerialConfig(string data) {
    Console.WriteLine("Received:");
    Console.WriteLine(data);
    //data.Remove('\n');
    /*var doc=JsonSerializer.Deserialize<JsonDocument>(data);
    var prefix=doc.RootElement.GetProperty("Prefix").ToString();
    if (!string.IsNullOrEmpty(prefix)) {
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

string CreateAndOutputProbeConfigOther() {
    ProbeControllerConfig probeControllerConfig = new ProbeControllerConfig() {
        ProbeConfigurations = {
            new ProbeConfig(new VoltageSensorConfig(54, 0.1), new CurrentSensorConfig(63, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(55, 0.1), new CurrentSensorConfig(64, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(56, 0.1), new CurrentSensorConfig(65, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(57, 0.1), new CurrentSensorConfig(66, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(58, 0.1), new CurrentSensorConfig(67, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(59, 0.1), new CurrentSensorConfig(68, 0.1))
        },
        ReadInterval = 100
    };
    MessagePacket msg = new MessagePacket() {
        Prefix = ArduinoMsgPrefix.ProbeConfigPrefix.Value,
        Packet = probeControllerConfig
    };
    var output=JsonSerializer.Serialize<MessagePacket>(msg,
    new JsonSerializerOptions { WriteIndented = false });
    return output;
    //await File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\MessagePacket.txt",output);
    //var doc=JsonSerializer.Deserialize<JsonDocument>(output);
    //var prefix=doc.RootElement.GetProperty("Prefix").ToString();
    /*if (prefix == ArduinoMsgPrefix.ProbeConfigPrefix.Value) {
        var received = doc.RootElement.GetProperty("Packet").Deserialize<ProbeControllerConfig>();
        Console.WriteLine($"Read Interval: {received.ReadInterval}");
    }*/
}

async Task CreateAndOutputProbeConfigFile() {
    ProbeControllerConfig probeControllerConfig = new ProbeControllerConfig() {
        ProbeConfigurations = {
            new ProbeConfig(new VoltageSensorConfig(54, 0.1), new CurrentSensorConfig(63, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(55, 0.1), new CurrentSensorConfig(64, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(56, 0.1), new CurrentSensorConfig(65, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(57, 0.1), new CurrentSensorConfig(66, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(58, 0.1), new CurrentSensorConfig(67, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(59, 0.1), new CurrentSensorConfig(68, 0.1))
        },
        ReadInterval = 100
    };
    MessagePacket msg = new MessagePacket() {
        Prefix = ArduinoMsgPrefix.ProbeConfigPrefix.Value,
        Packet = probeControllerConfig
    };
    var output=JsonSerializer.Serialize<MessagePacket>(msg,
    new JsonSerializerOptions { WriteIndented = false });
    //return output;
    await File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\MessagePacket.txt",output);
    //var doc=JsonSerializer.Deserialize<JsonDocument>(output);
    //var prefix=doc.RootElement.GetProperty("Prefix").ToString();
    /*if (prefix == ArduinoMsgPrefix.ProbeConfigPrefix.Value) {
        var received = doc.RootElement.GetProperty("Packet").Deserialize<ProbeControllerConfig>();
        Console.WriteLine($"Read Interval: {received.ReadInterval}");
    }*/
}

void CreateAndOutputProbeConfig(UsbController usb) {
    ProbeControllerConfig probeControllerConfig = new ProbeControllerConfig() {
        ProbeConfigurations = {
            new ProbeConfig(new VoltageSensorConfig(54, 0.1), new CurrentSensorConfig(63, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(55, 0.1), new CurrentSensorConfig(64, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(56, 0.1), new CurrentSensorConfig(65, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(57, 0.1), new CurrentSensorConfig(66, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(58, 0.1), new CurrentSensorConfig(67, 0.1)),
            new ProbeConfig(new VoltageSensorConfig(59, 0.1), new CurrentSensorConfig(68, 0.1))
        },
        ReadInterval = 100
    };
    MessagePacket msg = new MessagePacket() {
        Prefix = ArduinoMsgPrefix.ProbeConfigPrefix.Value,
        Packet = probeControllerConfig
    };
    usb.Send(msg);
    /*var output=JsonSerializer.Serialize<MessagePacket>(msg,
    new JsonSerializerOptions { WriteIndented = true });*/
    //await File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\MessagePacket.txt",output);
    //var doc=JsonSerializer.Deserialize<JsonDocument>(output);
    //var prefix=doc.RootElement.GetProperty("Prefix").ToString();
    /*if (prefix == ArduinoMsgPrefix.ProbeConfigPrefix.Value) {
        var received = doc.RootElement.GetProperty("Packet").Deserialize<ProbeControllerConfig>();
        Console.WriteLine($"Read Interval: {received.ReadInterval}");
    }*/
}

void CreateAndOutputHeaterConfig(UsbController controller) {
    NtcConfiguration ntcConfig1 = new NtcConfiguration(1.159e-3f, 1.429e-4f, 1.118e-6f, 60, 0.01);
    NtcConfiguration ntcConfig2 = new NtcConfiguration(1.173e-3f, 1.736e-4f, 7.354e-7f, 61, 0.01);
    NtcConfiguration ntcConfig3 = new NtcConfiguration(1.200e-3f, 1.604e-4f, 8.502e-7f, 62, 0.01);

    PidConfiguration pidConfig1 = new PidConfiguration(242.21,1868.81,128.49,250);
    PidConfiguration pidConfig2 = new PidConfiguration(765.77,1345.82,604.67,250);
    PidConfiguration pidConfig3 = new PidConfiguration(179.95,2216.84,81.62,250);

    HeaterConfiguration heaterConfig1 = new HeaterConfiguration(ntcConfig1, pidConfig1, 5, 3, 1);
    HeaterConfiguration heaterConfig2 = new HeaterConfiguration(ntcConfig2, pidConfig2, 5, 4, 1);
    HeaterConfiguration heaterConfig3 = new HeaterConfiguration(ntcConfig3, pidConfig3, 5, 5, 1);

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
    controller.Send(msgPacket);
    //await File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\HeaterConfig.txt",output);
    //Console.WriteLine("File Written");

}



public record ArduinoSerialData {
    public List<double> Voltages { get; set; } = new List<Double>();
    public List<double> Currents { get; set; } = new List<Double>();
    public List<double> Temperatures { get; set; } = new List<Double>();
    public List<long> ProbeRuntimes { get; set; } = new List<Int64>();
    public bool Heater1State { get; set; }
    public bool Heater2State { get; set; }
    public bool Heater3State { get; set; }
    public int CurrentSetPoint { get; set; }
    public int TemperatureSetPoint { get; set; }
    public long RuntimeSeconds { get; set; }
    public long ElapsedSeconds { get; set; }
    public bool Running { get; set; }
    public bool Paused { get; set; }
}




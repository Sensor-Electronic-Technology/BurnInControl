// See https://aka.ms/new-console-template for more information
using CP.IO.Ports;
using ReactiveMarbles.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using BurnIn.Shared.Models.Configurations;


Console.WriteLine("Outputting Probe Configurations");
await CreateAndOutputProbeConfig();
Console.WriteLine("Done");


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
    /*var startChar = 0x21.AsObservable();
    var endChar = 0x0D.AsObservable();*/

    var startChar = Convert.ToInt32('{').AsObservable();
    var endChar = Convert.ToInt32('}').AsObservable();
    
    var dis = new CompositeDisposable();
    var comdis = new CompositeDisposable();
    
    var port=new SerialPortRx("COM3", 38400);
    port.DisposeWith(comdis);
    port.ErrorReceived.Subscribe(Console.WriteLine).DisposeWith(comdis);
    //port.IsOpenObservable.Subscribe(x => Console.WriteLine($"Port {comPortName} is {(x ? "Open" : "Closed")}")).DisposeWith(comdis);
    port.DataReceived.BufferUntil(startChar, endChar, 100).Subscribe(HandleSerial).DisposeWith(comdis);
    //port.DataReceived.Subscribe(data => Console.WriteLine(data)).DisposeWith(comdis);
    port.Open();
    Console.WriteLine();
    while (true) {
        port.WriteLine(CreateData());
        Thread.Sleep(2000);
    }
    //Console.ReadLine();
    comdis.Dispose();
    dis.Dispose();
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

async Task CreateAndOutputProbeConfig() {
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
    
    var output=JsonSerializer.Serialize<ProbeControllerConfig>(probeControllerConfig,
    new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\probe_config.txt",output);
    Console.WriteLine("File Written");
}

async Task CreateAndOutputHeaterConfig() {
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
    configuration.HeaterConfigurations = new List<HeaterConfiguration>();
    configuration.HeaterConfigurations.Add(heaterConfig1);
    configuration.HeaterConfigurations.Add(heaterConfig1);
    configuration.HeaterConfigurations.Add(heaterConfig2);

    var output=JsonSerializer.Serialize<HeaterControllerConfiguration>(configuration,
    new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(@"C:\Users\aelmendo\Documents\HeaterConfig.txt",output);
    Console.WriteLine("File Written");

}

void HandleSerial(string data) {
    var aData=JsonSerializer.Deserialize<ArduinoSerialData>(data);
    foreach (var runtTime in aData.ProbeRuntimes) {
        Console.Write($"{runtTime} ,");
    }
    Console.WriteLine();
    //Console.WriteLine(data);
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




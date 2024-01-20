using BurnIn.Shared.Models;
using CP.IO.Ports;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using AsyncAwaitBestPractices;
namespace BurnIn.Shared.Controller;

public record UsbResult {
    public UsbState State { get; set; }
    public string? Message { get; set; }
    public UsbResult(UsbState state, string msg) {
        this.State = state;
        this.Message = msg;
    }
}

public record UsbWriteResult {
    public bool Success { get; set; }
    public UsbState State { get; set; }
    public string? Message { get; set; }
    public UsbWriteResult(bool success, UsbState state, string msg) {
        this.Success = success;
        this.State = state;
        this.Message = msg;
    }
}

public record UsbCommand(string Command);

public enum UsbState {
    Connected,
    Disconnected,
    Disposed
}

public class UsbController {
    //private ILogger<UsbController> _logger;
    private SerialPort? _serialPort;
    private readonly ChannelWriter<string> _channelWriter;
    private UsbState _state = UsbState.Disconnected;

    public UsbController(ChannelWriter<string> channelWriter) {
        //this._logger = logger;
        this._channelWriter = channelWriter;
    }

    public bool Connect() {
        try {
            this._serialPort=new SerialPort("COM3", 38400);
            this._serialPort.Open();
            this._state = this._serialPort?.IsOpen != null ? 
                UsbState.Connected : UsbState.Disconnected;
            return this._state == UsbState.Connected;
        } catch {
            this._state = UsbState.Disconnected;
            return false;
        }
    }

    public async Task StartReadingAsync(CancellationToken cancellationToken = default) {
        if (this._serialPort?.IsOpen != null && this._state == UsbState.Connected) {
            await Task.Run(async () => {
                Console.WriteLine($"{nameof(UsbController)} Thread = {Thread.CurrentThread.ManagedThreadId} " +
                                  $": Opening Stream on {this._serialPort.PortName} " +
                                  $"@ {this._serialPort.BaudRate}.");
                Console.WriteLine($"{nameof(UsbController)} " +
                                  $"Thread = {Thread.CurrentThread.ManagedThreadId} " +
                                  $": Reading Lines", 
                nameof(UsbController), 
                Thread.CurrentThread.ManagedThreadId);
                while (!cancellationToken.IsCancellationRequested && this._serialPort.ReadLine() is { } line) {
                    var startIndex=line.IndexOf('{');
                    if (startIndex >= 0) {
                        var input=line.Substring(startIndex, line.Length-startIndex);
                        await this._channelWriter.WriteAsync(input, cancellationToken);
                    }
                }
                Console.WriteLine($"{nameof(UsbController)} " +
                                  $"Thread = {Thread.CurrentThread.ManagedThreadId} " +
                                  $": CancellationRequested, " +
                                  $"completing channel and closing stream.");
                this._channelWriter.Complete();
                this._serialPort.Close();
                this._serialPort.Dispose();
                this._state = UsbState.Disposed;
            }, cancellationToken);
        }
    }
    public UsbWriteResult Send(MessagePacket msgPacket) {
        if (Monitor.TryEnter(this._serialPort, 1000)) {
            var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
            new JsonSerializerOptions(){WriteIndented = false});
            try {
                this._serialPort.Write(output);
            } catch { } finally {
                Monitor.Exit(this._serialPort);
            }
            return new UsbWriteResult(true,this._state, "");
        }
        return new UsbWriteResult(false, this._state, "");



        /* catch(Exception e) {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Error: Serial Write Failed Exception:");
            builder.AppendLine("Exception: "+e.Message);
            if (e.InnerException != null)
                builder.AppendLine("Inner Exception: " + e.InnerException.Message);
            return new UsbWriteResult(false,this._state,builder.ToString());
        }*/
    }
}

/*public class UsbController {
    public event EventHandler<UsbDataReceivedEventArgs>? SerialDataReceived;
    private SerialPort? _serialPort;
    private string _portName="";
    private UsbState _state=UsbState.Disposed;
    private bool _portNameFound = false;
    private ConcurrentQueue<string> _bufferQueue=new ConcurrentQueue<string>();
    private bool _continue=false;
    public bool IsConnected => this._serialPort?.IsOpen != null;
    private Thread _readThread;
    private Thread _handleThread;
    ManualResetEvent _readyEvent = new ManualResetEvent(false);
    private readonly ChannelWriter<string> _channelWriter;

    public UsbController(ChannelWriter<string> channelWriter) {
        this._channelWriter = channelWriter;
        this._readThread = new Thread(this.ReadThread);
        //this._handleThread = new Thread(this.HandleThread);
    }
    
    public UsbResult Connect(string? com=null) {
        StringBuilder builder=new StringBuilder();
        if (this._state != UsbState.Connected) {
            /*if (string.IsNullOrEmpty(com)) {
                this.FindPort();
            } else {
                this._portName = com;
                this._portNameFound = true;
            }#1#
            this._portNameFound = true;
            this._portName = "COM3";
            if (this._portNameFound) {
                try {
                    this._serialPort = new SerialPort(this._portName, 38400);
                    this._serialPort?.Open();
                    this._state = UsbState.Connected;
                    this._continue = true;
                    this._readThread.Start();
                    this._handleThread.Start();
                    return new UsbResult(this._state, "Controller Connected");
                } catch {
                    builder.AppendLine("Error: Failed to connect to controller");
                    this._state = UsbState.Disconnected;
                }
            }
            if (!this._portNameFound) {
                builder.AppendLine("Error: Arduino not found.  Is the device plugged in?");
            }

            if (this.SerialDataReceived == null) {
                builder.AppendLine("Error: Data Receive Event Not Wired");
            }
            return new UsbResult(this._state,builder.ToString());
        }
        return new UsbResult(this._state, "Device Already Connected");
    }
    public UsbResult Disconnect() {
        if (this._serialPort != null) {
            if (this._serialPort.IsOpen) {
                try {
                    this._serialPort.Close();
                    this._continue = false;
                    this._readThread.Join();
                    this._handleThread.Join();
                    this._serialPort.Close();
                    if (this._serialPort.IsOpen) {
                        this._state = UsbState.Connected;
                        return new UsbResult(this._state,"Error: Serial Port Not Closed!");
                    } else {
                        this._state = UsbState.Disconnected;
                        return new UsbResult(this._state,"Serial Port Closed");
                    }
                } catch(Exception e) {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("Error: An Exception was thrown while closing the serial port.  Exception: ");
                    builder.AppendLine(e.Message);
                    if (e.InnerException != null)
                        builder.AppendLine("Inner Exception: "+e.InnerException.Message);
                    return new UsbResult(this._state, builder.ToString());
                }
            }
            return new UsbResult(this._state,"Serial Port Already Closed");
        }
        return new UsbResult(this._state, "Serial Port Not Initialized or Open");
    }
    public UsbWriteResult Send(MessagePacket msgPacket) {
        if (this._serialPort?.IsOpen != null) {
            try {
                var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
                new JsonSerializerOptions(){WriteIndented = false});
                if (Monitor.TryEnter(this._serialPort,1000)) {
                    try {
                        this._serialPort.Write(output);
                    } finally {
                        Monitor.Exit(this._serialPort);
                    }
                }
                return new UsbWriteResult(true,this._state, "");
            } catch(Exception e) {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Error: Serial Write Failed Exception:");
                builder.AppendLine("Exception: "+e.Message);
                if (e.InnerException != null)
                    builder.AppendLine("Inner Exception: " + e.InnerException.Message);
                return new UsbWriteResult(false,this._state,builder.ToString());
            }
        }
        return new UsbWriteResult(false,this._state,$"Usb {this._state.ToString()}");
    }

    private void HandleThread() {
        while (this._continue) {
            this._readyEvent.WaitOne();
            if (this._bufferQueue.TryDequeue(out var message)) {
                Console.WriteLine(message);   
            }
        }
    }

    private void ReadThread() {
        while (this._continue) {
            string message=string.Empty;
            if (Monitor.TryEnter(this._serialPort, 500)) {
                try {
                    message = _serialPort.ReadLine();
                } catch(TimeoutException) {
                    //NOP
                } finally {
                    Monitor.Exit(this._serialPort);
                }
            }
            if (!string.IsNullOrEmpty(message)) {
                var startIndex=message.IndexOf('{');
                if (startIndex >= 0) {
                    var input=message.Substring(startIndex, message.Length-startIndex);
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
    
    private void FindPort() {
        Process process = new Process();
        string fileName =@"/home/setiburnin/Documents/test.sh";
        //Console.WriteLine();
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
        //Console.WriteLine($"Lines: {lines.Length}");
        var arduino=lines.FirstOrDefault(e => e.Contains("Arduino"));
        if (!string.IsNullOrEmpty(arduino)) {
            Console.WriteLine($"Found Arduino: ");
            int index=arduino.IndexOf('-');
            if (index >= 0) {
                var portName=arduino.Substring(0, index - 1);
                Console.WriteLine($"Port Name: {portName.Trim()}");
                this._portName=portName.Trim();
                this._portNameFound = true;
            } else {
                Console.WriteLine("Error Parsing Port Name");
                this._portNameFound = false;
            }
        } else {
            Console.WriteLine("Error: USB Not Found");
            this._portNameFound = false;
        }
    }
    

}*/
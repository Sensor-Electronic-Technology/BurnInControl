using BurnIn.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Channels;
using AsyncAwaitBestPractices;
using BurnIn.Shared.Models.BurnInStationData;
using System.Runtime.InteropServices;
using ThreadState=System.Threading.ThreadState;
namespace BurnIn.Shared.Controller;

public class UpperCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) {
        return char.ToUpper(name[0]) + name.Substring(1, name.Length - 1);
    }
       
}

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
public enum UsbState {
    Connected,
    Disconnected,
    Disposed,
    Unknown
}

public class UsbController:IDisposable {
    public event EventHandler UsbUnPlogHandler;
    
    private readonly ILogger<UsbController> _logger;
    private bool _loggingEnabled=false;
    private readonly SerialPort _serialPort;
    private readonly ChannelWriter<string> _channelWriter;
    private UsbState _state = UsbState.Disconnected;
    private Thread _readThread;
    private bool _continue=false;
    private bool _portNameFound = false;
    private CancellationToken _cancellationToken;
    public bool Connected => this._serialPort.IsOpen;
    public UsbController(ChannelWriter<string> channelWriter,ILogger<UsbController> logger) {
        this._logger = logger;
        this._channelWriter = channelWriter;
        this._serialPort = new SerialPort();
        this._loggingEnabled = true;
    }

    public UsbController(ChannelWriter<string> channelWriter) {
        this._loggingEnabled = false;
        this._channelWriter = channelWriter;
        this._serialPort = new SerialPort();
    }

    public UsbResult Connect() {
        if (this._serialPort.IsOpen) {
            return new UsbResult(UsbState.Connected, "Usb already connected");
        }
        string portName = string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            portName=this.FindPort();
            if (!string.IsNullOrEmpty(portName)) {
                this._portNameFound = true;
            }
        } else {
            //Debugging
            portName = this.FindPortWindows();
            this._portNameFound = true;
        }
        if (this._portNameFound) {
            this._serialPort.PortName = portName;
            this._serialPort.BaudRate = 38400;
            try {
                this._serialPort.Open();
                this._state = this._serialPort?.IsOpen != null ? 
                    UsbState.Connected : UsbState.Disconnected;
                this._readThread = new Thread(this.ReadThread);
                this._continue = true;
                this._readThread.Start();
                return new UsbResult(this._state, "Usb connected and reading");
            } catch(Exception exception) {
                string message = $"Error: Usb Connection failed. Error: {exception.Message}";
                if (exception.InnerException != null) {
                    message += $" \\n Inner:{exception.InnerException.Message} ";
                }
                this._state = UsbState.Disconnected;
                this.Log(message,true);
                return new UsbResult(this._state,message);
            }
        } else {
            return new UsbResult(this._state,"Error: Failed to find usb port with the controller");
        }
    }

    public UsbResult Disconnect() {
        if (!this._serialPort.IsOpen) {
            this._continue = false;
            if (this._readThread.ThreadState==ThreadState.Running) {
                this._readThread.Join();
            }
            this._state = UsbState.Disconnected;
            return new UsbResult(this._state, "Usb already disconnected");
        }
        try {
            this._continue = false;
            this._readThread.Join();
            this._channelWriter.Complete();
            this._serialPort.Close();
            this._state = this._serialPort.IsOpen ? 
                UsbState.Connected : UsbState.Disconnected;
            return new UsbResult(this._state, "Usb should be disconnected");
        } catch {
            this._state = UsbState.Unknown;
            return new UsbResult(this._state,"Error: Usb disconnect failed, state unknown!");
        }
    }
    private void ReadThread() {
        this.Log($"{nameof(UsbController)} Thread = {Thread.CurrentThread.ManagedThreadId} " +
                 $": Opening Stream on {this._serialPort.PortName} " +
                 $"@ {this._serialPort.BaudRate}.",false);
        this.Log($"{nameof(UsbController)} " +
                 $"Thread = {Thread.CurrentThread.ManagedThreadId} " +
                 $": Reading Lines",false);
        while (this._continue) {
            string line = string.Empty;
            if (Monitor.TryEnter(this._serialPort)) {
                if (this._serialPort.IsOpen && this._state == UsbState.Connected) {
                    try {
                        line = this._serialPort.ReadLine();
                    } catch(Exception e) {
                        this._state = UsbState.Disconnected;
                        this._continue = false;
                        this.UsbUnPlogHandler?.Invoke(this,EventArgs.Empty);
                    } finally {
                        Monitor.Exit(this._serialPort);
                    }
                }
            }
            if (!string.IsNullOrEmpty(line)) {
                var startIndex=line.IndexOf('{');
                if (startIndex >= 0) {
                    var input=line.Substring(startIndex, line.Length-startIndex);
                    if (!this._channelWriter.TryWrite(input)) {
                        this.Log("Channel Write Failed",true);
                    }
                }
            }
        }
        this.Log($"{nameof(UsbController)} " +
                 $"Thread = {Thread.CurrentThread.ManagedThreadId} " +
                 $": Closing",false);
    }
    public UsbWriteResult SendV2<TPacket>(MessagePacketV2<TPacket> msgPacket) where TPacket:IPacket {
        var output = JsonSerializer.Serialize(msgPacket,
        new JsonSerializerOptions() {
            PropertyNamingPolicy =null,
            WriteIndented = false
        });
        Monitor.Enter(this._serialPort);
        try {
            this._serialPort.Write(output);
        } catch(Exception e) {
            return new UsbWriteResult(false,this._state, $"Exception Caught: {e.Message}");
        }finally {
            Monitor.Exit(this._serialPort);
        }
        return new UsbWriteResult(true,this._state, "");
    }
    private string FindPort() {
        Process process = new Process();
        //string fileName =@"/home/setiburnin/Documents/test.sh";
        //Console.WriteLine();
        ProcessStartInfo startInfo = new ProcessStartInfo { 
            FileName = "/bin/bash",
            Arguments = Environment.CurrentDirectory+"\\ListUsbPorts.sh",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        process.StartInfo = startInfo;
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        var lines=result.Split('\n');
        var arduino=lines.FirstOrDefault(e => e.Contains("Arduino"));
        if (!string.IsNullOrEmpty(arduino)) {
            int index=arduino.IndexOf('-');
            if (index >= 0) {
                var portName=arduino.Substring(0, index - 1);
                this.Log($"Found Arduino: {portName}",false);
                return portName;
            } else {
                return string.Empty;
            }
        } else {
            return string.Empty;
        }
    }
    private string FindPortWindows() {
        return "COM3";
    }
    public void Dispose() {
        this._continue = false;
        if (this._readThread.IsAlive) {
            this._readThread.Join();
        }
        this._serialPort.Dispose();
        //this._channelWriter.Complete();
    }

    private void Log(string message,bool error) {
        if (this._loggingEnabled) {
            if (error) {
                this._logger.LogError(message);
            } else {
                this._logger.LogInformation(message);
            }
        } else {
            Console.Write(message);
        }

    }
}


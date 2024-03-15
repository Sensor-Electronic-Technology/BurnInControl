using BurnInControl.Shared.ComDefinitions.MessagePacket;
using ErrorOr;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Ports;
using SerialPortLib;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ThreadState=System.Threading.ThreadState;
namespace StationService.Infrastructure.SerialCom;


public class UsbController:IDisposable {
    public event EventHandler UsbUnPlugHandler;
    
    private readonly ILogger<UsbController> _logger;
    private bool _loggingEnabled=false;
    private readonly SerialPortInput _serialPort;
    private readonly ChannelWriter<string> _channelWriter;
    private bool _portNameFound = false;
    private string _portName = string.Empty;
    private int _baudRate = 38400;
    private StringBuilder _inputBuffer=new StringBuilder();
    public bool Connected => this._serialPort.IsConnected;
    
    public UsbController(ChannelWriter<string> channelWriter,ILogger<UsbController> logger) {
        this._logger = logger;
        this._channelWriter = channelWriter;
        this._serialPort = new SerialPortInput();
        this._loggingEnabled = true;
        this._inputBuffer.Clear();
        this._serialPort.MessageReceived+= SerialPortOnMessageReceived;
    }

    public UsbController(ChannelWriter<string> channelWriter) {
        this._loggingEnabled = false;
        this._channelWriter = channelWriter;
        this._serialPort = new SerialPortInput();
        this._inputBuffer.Clear();
        this._serialPort.MessageReceived+= SerialPortOnMessageReceived;
    }
    private void SerialPortOnMessageReceived(Object sender, MessageReceivedEventArgs args) {
        var input=System.Text.Encoding.ASCII.GetString(args.Data);

        this._inputBuffer.Append(input);
        
        if (input.Contains('\n')) {
            var output = this._inputBuffer.ToString();
            var startIndex=output.IndexOf('{');
            if (startIndex >= 0) {
                if (this._channelWriter.TryWrite(output.Substring(startIndex, output.Length-startIndex))) {
                    this._inputBuffer.Clear();
                    return;
                } else {
                    this._inputBuffer.Clear();
                    this.Log($"Channel Write Failed, ThreadId: {Thread.CurrentThread.ManagedThreadId}",true);
                    return;
                }
            }
            this._inputBuffer.Clear();
        }
    }

    public ErrorOr<Success> Connect() {
        /*if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            this._portName = "/dev/ttyACM0";
            if (!string.IsNullOrEmpty(this._portName)) {
                this._portNameFound = true;
            }
        } else {
            //Debugging
            this._portName = this.FindPortWindows();
            this._portNameFound = true;
        }*/
        this._portName = "COM3";
        this._portNameFound = true;
        if (this._portNameFound) {
            this._serialPort.SetPort(this._portName, 38400);
            if (this._serialPort.Connect()) {
                this.Log("Usb Connected",false);
            } else {
                this.Log("USB Failed to Connect", true);
            }
            return this._serialPort.IsConnected ? Result.Success: Error.Failure(description: "Failed to connect");
        }else {
            return Error.NotFound(description: "SerialPort not found");
        }
    }

    public ErrorOr<Success> Disconnect() {
        if (!this._serialPort.IsConnected) {
            return Error.Conflict(description: "Usb already disconnected");
        }
        try {
            this._serialPort.Disconnect();
            return Result.Success;
        } catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            this.Log($"Usb failed to disconnect.  Exception was thrown \n {exMessage}",true);
            return Error.Failure(description: $"Usb failed to disconnect.  Exception was thrown \n {exMessage}");
        }
    }
    
    public ErrorOr<Success> Stop() {
        if (!this._serialPort.IsConnected) {

            return Result.Success;
        }
        try {
            this._channelWriter.Complete();
            this._serialPort.Disconnect();
            return Result.Success;
        }  catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            this.Log($"Controller did not stop cleanly.  Exception was thrown \n {exMessage}",true);
            return Error.Failure(description: $"Failed to cleanly stop.  Exception was thrown \n {exMessage}");
        }
    }
    
    public ErrorOr<Success> Send<TPacket>(MessagePacket<TPacket> msgPacket) where TPacket:IPacket {
        var output = JsonSerializer.Serialize(msgPacket,
        new JsonSerializerOptions() {
            PropertyNamingPolicy =null,
            WriteIndented = false
        });
        Monitor.Enter(this._serialPort);
        try {
            this._serialPort.SendMessage(System.Text.Encoding.ASCII.GetBytes(output));
        } catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            this.Log($"Failed to send,Exception: {exMessage}",true);
            return Error.Failure(description: $"Failed to send, Exception was thrown. \n {exMessage}");
        }finally {
            Monitor.Exit(this._serialPort);
        }
        return Result.Success;
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
        this._channelWriter.TryComplete();
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

/*public class UsbController:IDisposable {
    public event EventHandler UsbUnPlugHandler;
    
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

    public ErrorOr<Success> Connect() {
        /*if (this._serialPort.IsOpen) {
            return Error.Conflict(description: "Usb already connected");
        }#1#
        //string portName = string.Empty;
        //string portName = "/dev/ttyACM0";
        string portName = "COM3";
        this._portNameFound = true;
        /*if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            portName = "/dev/ttyACM0";
            if (!string.IsNullOrEmpty(portName)) {
                this._portNameFound = true;
            }
        } else {
            //Debugging
            portName = this.FindPortWindows();
            this._portNameFound = true;
        }#1#
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
                return Result.Success;
            } catch(Exception exception) {
                string exMessage = "Exception: "+exception.Message;
                if (exception.InnerException != null) {
                    exMessage += $" \n Inner:{exception.InnerException.Message} ";
                }
                this._state = UsbState.Disconnected;
                this.Log($"Failed to connect. Exception was thrown. \n Exception: {exMessage}",true);
                return Error.Failure(description: $"Failed to connect. Exception was thrown. \n Exception: {exMessage}");
            }
        }else {
            return Error.NotFound(description: "SerialPort not found");
        }
    }

    public ErrorOr<Success> Disconnect() {
        if (!this._serialPort.IsOpen) {
            this._continue = false;
            if (this._readThread.ThreadState==ThreadState.Running) {
                this._readThread.Join();
            }
            this._state = UsbState.Disconnected;
            return Error.Conflict(description: "Usb already disconnected");
        }
        try {
            this._continue = false;
            this._readThread.Join();
            this._serialPort.Close();
            this._state = this._serialPort.IsOpen ? 
                UsbState.Connected : UsbState.Disconnected;
            return Result.Success;
        } catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            this._state = UsbState.Unknown;
            this.Log($"Usb failed to disconnect.  Exception was thrown \n {exMessage}",true);
            return Error.Failure(description: $"Usb failed to disconnect.  Exception was thrown \n {exMessage}");
        }
    }
    
    public ErrorOr<Success> Stop() {
        if (!this._serialPort.IsOpen) {
            this._continue = false;
            if (this._readThread.ThreadState==ThreadState.Running) {
                this._readThread.Join();
            }
            this._state = UsbState.Disconnected;
            return Result.Success;
        }
        try {
            this._continue = false;
            this._readThread.Join();
            this._channelWriter.Complete();
            this._serialPort.Close();
            this._state = this._serialPort.IsOpen ? 
                UsbState.Connected : UsbState.Disconnected;
            return Result.Success;
        }  catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            this._state = UsbState.Unknown;
            this.Log($"Controller did not stop cleanly.  Exception was thrown \n {exMessage}",true);
            return Error.Failure(description: $"Failed to cleanly stop.  Exception was thrown \n {exMessage}");
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
                        this.UsbUnPlugHandler?.Invoke(this,EventArgs.Empty);
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
    public ErrorOr<Success> Send<TPacket>(MessagePacket<TPacket> msgPacket) where TPacket:IPacket {
        var output = JsonSerializer.Serialize(msgPacket,
        new JsonSerializerOptions() {
            PropertyNamingPolicy =null,
            WriteIndented = false
        });
        Monitor.Enter(this._serialPort);
        try {
            this._serialPort.Write(output);
        } catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            this.Log($"Failed to send,Exception: {exMessage}",true);
            return Error.Failure(description: $"Failed to send, Exception was thrown. \n {exMessage}");
        }finally {
            Monitor.Exit(this._serialPort);
        }
        return Result.Success;
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
        this._channelWriter.TryComplete();
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
}*/
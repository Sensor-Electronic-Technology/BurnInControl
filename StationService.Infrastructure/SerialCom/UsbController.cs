using BurnInControl.Shared.ComDefinitions.MessagePacket;
using ErrorOr;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Ports;
using SerialPortLib;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ThreadState=System.Threading.ThreadState;
namespace StationService.Infrastructure.SerialCom;


public class UsbController:IDisposable {
    public event EventHandler<ConnectionStatusChangedEventArgs>? OnUsbStateChangedHandler;
    
    private readonly ILogger<UsbController> _logger;
    private bool _loggingEnabled=false;
    private readonly SerialPortInput _serialPort;
    private readonly ChannelWriter<string> _channelWriter;
    private bool _portNameFound = false;
    private string _portName = string.Empty;
    private readonly int _baudRate = 38400;
    public bool Connected => this._serialPort.IsConnected;
    
    public UsbController(ChannelWriter<string> channelWriter,ILogger<UsbController> logger) {
        this._logger = logger;
        this._channelWriter = channelWriter;
        this._serialPort = new SerialPortInput(true);
        this._loggingEnabled = true;

        this._serialPort.MessageLineReceived+= SerialPortOnMessageLineReceived;
        this._serialPort.ConnectionStatusChanged+= SerialPortOnConnectionStatusChanged;
        this._serialPort.ReconnectDelay = 1000;
    }
    public UsbController(ChannelWriter<string> channelWriter) {
        this._loggingEnabled = false;
        this._channelWriter = channelWriter;
        this._serialPort = new SerialPortInput(true);
        this._serialPort.MessageLineReceived+= SerialPortOnMessageLineReceived;
        this._serialPort.ConnectionStatusChanged+= SerialPortOnConnectionStatusChanged;
        this._serialPort.ReconnectDelay = 1000;
    }
    private void SerialPortOnMessageLineReceived(Object sender, MessageReceivedLineEventArgs args) {
        if (args.Data.Contains('{')) {
            if (args.Data.IndexOf('{') == 0) {
                if (!this._channelWriter.TryWrite(args.Data)){
                    this.Log($"Channel Write Failed, ThreadId: {Thread.CurrentThread.ManagedThreadId}",true);
                }
            }
        }
    }
    private void SerialPortOnConnectionStatusChanged(Object sender, ConnectionStatusChangedEventArgs args) {
        this.OnUsbStateChangedHandler?.Invoke(this,args);
    }
    
    public ErrorOr<Success> Connect() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            this._portName = "/dev/ttyACM0";
            //this._portName=this.FindPort();
            if (!string.IsNullOrEmpty(this._portName)) {
                this._portNameFound = true;
            }
        } else {
            //Debugging
            this._portName = this.FindPortWindows();
            this._portNameFound = true;
        }
        /*this._portName = "/dev/ttyACM0";
        this._portNameFound = true;*/
        if (this._portNameFound) {
            this._serialPort.SetPort(this._portName, this._baudRate);
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
        Monitor.Enter(this._serialPort);//Lock object for thread safety
        try {
            this._serialPort.SendMessage(System.Text.Encoding.ASCII.GetBytes(output));//send bytes
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
        /*ProcessStartInfo startInfo = new ProcessStartInfo { 
            FileName = "/bin/bash",
            Arguments = Environment.CurrentDirectory+"\\ListUsbPorts.sh",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };*/
        ProcessStartInfo startInfo = new ProcessStartInfo { 
            FileName = "./ListUsbPorts.sh",
            /*Arguments = Environment.CurrentDirectory+"\\ListUsbPorts.sh",*/
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
        //return "COM3";
        return "COM4";
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
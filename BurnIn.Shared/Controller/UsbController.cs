using BurnIn.Shared.Models;
using CP.IO.Ports;
using ReactiveMarbles.Extensions;
using System.Diagnostics;
using System.IO.Ports;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
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

public class UsbDataReceivedEventArgs : EventArgs {
    public string? Data { get; set; }
}

public class UsbController {
    //public event SerialDataReceivedEventHandler? SerialDataReceived;
    //private Action<string>? _dataReceivedDelegate;
    public event EventHandler<UsbDataReceivedEventArgs>? SerialDataReceived;
    private SerialPortRx? _serialPort;
    private string _portName="";
    private UsbState _state=UsbState.Disposed;
    private bool _portNameFound = false;
    private IObservable<char> _startChar = Convert.ToInt32('{').AsObservable();
    private IObservable<char> _endChar = Convert.ToInt32('\n').AsObservable();
    private CompositeDisposable disposable=new CompositeDisposable();
    
    public bool IsConnected => this._serialPort?.IsOpen != null;
    
    public UsbResult Connect(string? com=null) {
        StringBuilder builder=new StringBuilder();
        if (this._state != UsbState.Connected) {
            /*if (string.IsNullOrEmpty(com)) {
                this.FindPort();
            } else {
                this._portName = com;
                this._portNameFound = true;
            }*/
            this._portNameFound = true;
            
            if (this._portNameFound && this.SerialDataReceived!=null) {
                this._serialPort = new SerialPortRx(com, 38400);
                var portList = SerialPort.GetPortNames();
                Console.WriteLine("Port List: ");
                foreach (var port in portList) {
                    Console.WriteLine(port);
                }
                this._serialPort.DisposeWith(this.disposable);
                this._serialPort.ErrorReceived.Subscribe(Console.WriteLine)
                    .DisposeWith(this.disposable);
                this._serialPort.DataReceived.BufferUntil(this._startChar, this._endChar, 200)
                    .Subscribe(data=>this.SerialDataReceived.Invoke(this,new UsbDataReceivedEventArgs(){Data=data}))
                    .DisposeWith(this.disposable);
                try {
                    this._serialPort.Open();
                    //bool open=this._serialPort.IsOpenObservable.Wait();
                        /*.Subscribe(data=>Console.WriteLine($"IsOpen: {data}"))
                        .DisposeWith(this.disposable);*/
                        Console.WriteLine($"IsOpen: {this._serialPort.IsOpen}");
                    /*if (this._serialPort.IsOpen) {
                        builder.AppendLine("Usb Connected");
                        this._state = UsbState.Connected;
                        return new UsbResult(this._state, builder.ToString());
                    } else {
                        this._state = UsbState.Disconnected;
                        builder.AppendLine("Error: Failed To Open Serial Port, no except");
                        return new UsbResult(this._state, builder.ToString());
                    }*/
                    return new UsbResult(UsbState.Connected,"Maybe");
                } catch(Exception e) {
                    this._state = UsbState.Disconnected;
                    builder.AppendLine("Error: Failed To Open Serial Port");
                    builder.AppendLine("Exception: "+e.Message);
                    if (e.InnerException != null)
                        builder.AppendLine("Inner Exception: " + e.InnerException.Message);

                    return new UsbResult(this._state, builder.ToString());
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
                    if (this._serialPort.IsOpen) {
                        this._state = UsbState.Connected;
                        return new UsbResult(this._state,"Error: Serial Port Not Closed!");
                    } else {
                        //this.disposable.Dispose();
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
                var serializedMsgPacket=JsonSerializer.Serialize(msgPacket);
                this._serialPort.WriteLine(serializedMsgPacket);
                //Console.WriteLine("MsgPacket Sent:");
                //Console.WriteLine(serializedMsgPacket);
                return new UsbWriteResult(true,this._state, "");
            } catch(Exception e) {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Error: Serial Write Failed Exception:");
                builder.AppendLine("Exception: "+e.Message);
                if (e.InnerException != null)
                    builder.AppendLine("Inner Exception: " + e.InnerException.Message);
                return new UsbWriteResult(false,this._state,"");
            }
        }
        return new UsbWriteResult(false,this._state,$"Usb {this._state.ToString()}");
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
    

}
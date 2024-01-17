using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models.BurnInStationData;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
namespace BurnIn.Shared.Controller;

public record ControllerResult(bool Success, string? Message) {
    public bool Success { get; set; } = Success;
    public string? Message { get; set; } = Message;
}

public class StationController {
    private readonly UsbController _usbController;
    private readonly ILogger<StationController> _logger;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly SerialBuffer _serialBuffer = new SerialBuffer();
    private readonly Queue<string> _messageBuffer = new Queue<String>();
    private RawReading _reading = new RawReading();
    private Timer _broadcastTimer,_logTimer;
    private bool _running = false;
    

    public StationController(IHubContext<StationHub,
            IStationHub> hubContext, 
        UsbController usbController,
        ILoggerFactory loggerFactory) {
        
        this._logger = loggerFactory.CreateLogger<StationController>();
        this._usbController = usbController;
        //this._usbController.SerialDataReceived += this.DataReceivedHandler;
        this._hubContext = hubContext;
    }

    public Task Start() {
        //load data
        //this._usbController.Connect();
        return Task.CompletedTask;
    }
    
    public Task<ControllerResult> ConnectUsb() {
        var result=this._usbController.Connect();
        if (result.State==UsbState.Connected) {
            this._broadcastTimer = new Timer(this.SendCom, null, 1000, 1000);
        }
        return Task.FromResult(new ControllerResult(
            result.State==UsbState.Connected,
            result.Message));
    }

    public Task<ControllerResult> Disconnect() {
        var result=this._usbController.Disconnect();
        return Task.FromResult(new ControllerResult(result.State==UsbState.Disconnected,result.Message));
    }

    /*public Task<ControllerResult> ExecuteCommand(ArduinoCommand command,string? commandstr=null) {
            ControllerResult controllerResult=new ControllerResult(false,"Error: Invalid Command");
            command.When(ArduinoCommand.Start).Then(() => {
                if (!this._reading.running) {
                    var result = this._usbController.Send(command.Value);
                    if (result.Success) {
                        //create log
                        this._logTimer = new Timer(this.Log, null, 5000, 5000);
                    }
                    controllerResult = new ControllerResult(result.Success, result.Message);
                } else {
                    controllerResult = new ControllerResult(false, "Test Already Running");
                }
            }).When(ArduinoCommand.Pause).Then(() => {
                if (this._reading is { running: true, paused: false }) {
                    var result = this._usbController.Send(command.Value);
                    controllerResult = new ControllerResult(result.Success, result.Message);
                } else {
                    controllerResult = new ControllerResult(false, "Test isn't running or already paused");
                }
            }).When(ArduinoCommand.Reset).Then(() => {
                var result = this._usbController.Send(command.Value);
                controllerResult = new ControllerResult(result.Success, result.Message);
            }).When(ArduinoCommand.Test).Then(() => {
                if (this._reading is { running: false, paused: false }) {
                    var result = this._usbController.Send(command.Value);
                    controllerResult = new ControllerResult(result.Success, "Test Executing");
                } else {
                    controllerResult = new ControllerResult(false, "Error: Test Running");
                }
            }).When(ArduinoCommand.CurrentToggle).Then(() => {
                if (this._reading is { running: false, paused: false }) {
                    var result = this._usbController.Send(command.Value);
                    controllerResult = new ControllerResult(result.Success, "Current Toggled");
                } else {
                    controllerResult = new ControllerResult(false, "Error: Test running");
                }
            }).When(ArduinoCommand.HeaterToggle).Then(() => {
                if (this._reading is { running: false, paused: false }) {
                    var result = this._usbController.Send(command.Value);
                    controllerResult = new ControllerResult(result.Success, "Heater Toggled");
                } else {
                    controllerResult = new ControllerResult(false, "Error: Test Running");
                }
            });
            return Task.FromResult(controllerResult);
    }*/

    /*public Task<ControllerResult> UpdateArduinoSettings(string command) {
        var usbResult=this._usbController.WriteCommand(command);
        if (usbResult.Success) {
            return Task.FromResult(new ControllerResult(true,"Settings Sent"));
        } else {
            return Task.FromResult(new ControllerResult(false,"Error: Settings Failed To Upload "));
        }
    }*/

    private async void SendCom(object? state) {
        if (this._messageBuffer.TryDequeue(out var msg)) {
            await this._hubContext.Clients.All.OnSerialComMessage(msg);
        } else {
            await this._hubContext.Clients.All.OnSerialCom(this._reading);
        }
        Console.WriteLine("Controller Send Com");

    }

    private async void Log(object? state) {
        if (this._reading.running && !this._reading.paused) {
            //log
        }
    }
    
    private void DataReceivedHandler(
        object sender, SerialDataReceivedEventArgs e) {
        SerialPort serialPort = (SerialPort)sender;
        try {
            SerialPort sp = (SerialPort)sender;
            if(serialPort.BytesToRead > 1) {
                string indexStr = serialPort.ReadTo("[");
                string valueStr = "";
                char command = Convert.ToChar(serialPort.ReadChar());
                switch (command) {
                    case 'R': {
                        indexStr = serialPort.ReadTo("]");
                        valueStr = serialPort.ReadTo("{");
                        valueStr = serialPort.ReadTo("}");
                        if (double.TryParse(valueStr, out var value) && int.TryParse(indexStr,out var rIndex)) {
                            this._serialBuffer.RealArray[rIndex] = value;
                        }
                        break;
                    }
                    case 'B': {
                        indexStr = serialPort.ReadTo("]");
                        valueStr = serialPort.ReadTo("{");
                        valueStr = serialPort.ReadTo("}");
                        if (int.TryParse(indexStr, out var index)) {
                            if (valueStr.Contains("0")) {
                                this._serialBuffer.BoolArray[index] = false;
                            }else if (valueStr.Contains("1")) {
                                this._serialBuffer.BoolArray[index] = true;
                            }
                        }
                        break;
                    }
                    case 'T': {
                        indexStr = serialPort.ReadTo("]");
                        valueStr = serialPort.ReadTo("{");
                        valueStr = serialPort.ReadTo("}");
                        this._hubContext.Clients.All.OnSerialComMessage(valueStr).Wait();
                        //this._messageBuffer.Enqueue(valueStr);
                        break;
                    }
                    default:break;
                }
                
                this._reading.v11 = this._serialBuffer.RealArray[0];
                this._reading.v12 = this._serialBuffer.RealArray[1];
                this._reading.v21 = this._serialBuffer.RealArray[2];
                this._reading.v22 = this._serialBuffer.RealArray[3];
                this._reading.v31 = this._serialBuffer.RealArray[4];
                this._reading.v32 = this._serialBuffer.RealArray[5];

                this._reading.t1 = this._serialBuffer.RealArray[6];
                this._reading.t2 = this._serialBuffer.RealArray[7];
                this._reading.t3 = this._serialBuffer.RealArray[8];

                this._reading.elapsed = Convert.ToInt64(this._serialBuffer.RealArray[9]);
                this._reading.temperatureSP=this._serialBuffer.RealArray[10];
                this._reading.currentSP = this._serialBuffer.RealArray[11];
                
                this._reading.i11 = this._serialBuffer.RealArray[12];
                this._reading.i12 = this._serialBuffer.RealArray[13];
                this._reading.i21 = this._serialBuffer.RealArray[14];
                this._reading.i22 = this._serialBuffer.RealArray[15];
                this._reading.i31 = this._serialBuffer.RealArray[16];
                this._reading.i32 = this._serialBuffer.RealArray[17];

                this._reading.runTime = Convert.ToInt64(this._serialBuffer.RealArray[18]);
                
                this._reading.running = this._serialBuffer.BoolArray[0];
                this._reading.heating1 = this._serialBuffer.BoolArray[1];
                this._reading.heating2 = this._serialBuffer.BoolArray[2];
                this._reading.heating3 = this._serialBuffer.BoolArray[3];
                this._reading.paused = this._serialBuffer.BoolArray[4];
                
                if (this._reading.running && !this._running) {
                    this._running = true;
                    //log start
                }else if (!this._reading.running && this._running) {
                    this._running = false;
                    //log stop
                }
                /*Console.Write("R: ");
                for (int i = 0; i < 19; i++) {
                    Console.Write($"{this._serialBuffer.RealArray[i]},");
                }
                Console.Write(" B: ");
                for (int i = 0; i < 5; i++) {
                    Console.Write($"{this._serialBuffer.BoolArray[i]},");
                }
                Console.WriteLine();*/
            }
        }catch{}
    }
}

public record SerialBuffer {
    public double[] RealArray { get; set; } = new double[100];
    public bool[] BoolArray { get; set; } = new bool[100];
}
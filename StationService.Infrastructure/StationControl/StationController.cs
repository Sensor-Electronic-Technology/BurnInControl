using AsyncAwaitBestPractices;
using BurnInControl.Application.ProcessSerial.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.HubDefinitions.Hubs;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SerialPortLib;
using StationService.Infrastructure.Hub;
using StationService.Infrastructure.SerialCom;
using System.Threading.Channels;
using Stateless;
using System.Timers;
using Timer=System.Timers.Timer;
namespace StationService.Infrastructure.StationControl;

public enum ControllerState {
    NotDefined,
    Startup,
    TryConnect,
    TryDisconnect,
    Idle,
    Running,
    Paused,
    Connected,
    NotConnected,
    Stop,
    Error
}

public enum ControllerTrigger {
    Connect,
    Connected,
    StartTest,
    PauseTest,
    ContinueTest,
    NotConnected,
    Disconnect,
    Startup,
    Reset
}

public class ControllerStateData {
    public bool IsConfigured { get; set; }
    public bool IsConnected { get; set; }
    public string? StationId { get; set; }
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public ControllerStateData(bool configure=false) {
        this.IsConfigured = configure;
        this.IsConnected = false;
        this.IsError = false;
    }
}

public class StationController:IStationController,IDisposable {
    private readonly UsbController _usbController;
    private readonly ILogger<StationController> _logger;
    private readonly ChannelReader<string> _channelReader;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ISender _sender;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    /*private readonly StateMachine<ControllerState, ControllerTrigger> _stateMachine;
    private ControllerStateData _stateData=new ControllerStateData();
    private Timer _retryTimer;*/
    
    
    public StationController(UsbController usbController,
        ChannelReader<string> channelReader,
        ISender sender,
        IOptions<StationSettings> _options,
        IHubContext<StationHub, IStationHub> hubContext,
        ILogger<StationController> logger) {
        this._logger = logger;
        this._channelReader = channelReader;
        this._usbController = usbController;
        this._usbController.OnUsbStateChangedHandler+= UsbControllerOnUsbStateChangedHandler;
        this._sender = sender;
        this._hubContext = hubContext;
        /*this._stateData.StationId = _options.Value.StationId;
        this._stateMachine = new StateMachine<ControllerState, ControllerTrigger>(ControllerState.NotDefined);
        this._retryTimer = new Timer(TimeSpan.FromSeconds(10));
        this._retryTimer.AutoReset = true;
        this._retryTimer.Elapsed+= RetryTimerOnElapsed;*/
    }
    private void RetryTimerOnElapsed(Object? sender, ElapsedEventArgs e) {
        /*if (CanFire(ControllerTrigger.Connect)) {
            this._stateMachine.FireAsync(ControllerTrigger.Connect)
                .SafeFireAndForget();
        } else {
            this._stateMachine.PermittedTriggers
                .ToList()
                .ForEach(trigger => this._logger.LogInformation("Permitted Trigger: {Trigger}", trigger));
        }*/
    }

    public async Task Start() {
        //await this.BuildStateMachine();
        await this.ConnectUsb();
    }

    public Task GetConnectionStatus() {
       return this._hubContext.Clients.All.OnStationConnection(this._usbController.Connected);
    }

    public async Task<ErrorOr<Success>> ConnectUsb() {
        var result=this._usbController.Connect();
        if (!result.IsError) {
            this.StartReaderAsync(this._cancellationTokenSource.Token)
                .SafeFireAndForget(e => {
                    var message = $"Channel read failed. Exception: \n {e.Message}";
                    if(e.InnerException!=null) {
                        message+= $"\n Inner Exception: {e.InnerException.Message}";
                    }
                    this._hubContext.Clients.All.OnUsbConnectFailed(message);
                    this._logger.LogError(message);
                });
            //await this._stateMachine.FireAsync(ControllerTrigger.Connected);
            /*return Task.FromResult<ErrorOr<Success>>(result.Value);*/
            return result;
        } else {
            this._hubContext.Clients.All.OnUsbConnectFailed($"Usb failed to connect.  " +
                                                            $"Please check usb cable" +
                                                            $"\n Usb Message: {result.FirstError.Description})")
                .SafeFireAndForget();
            /*this._stateMachine.FireAsync(ControllerTrigger.NotConnected)
                .SafeFireAndForget();*/
            return result;
        }
    }

    public Task<ErrorOr<Success>> Disconnect() {
        var result=this._usbController.Disconnect();
        if (result.IsError) {
            this._hubContext.Clients.All.OnUsbDisconnectFailed($"Usb failed to disconnect.  " +
                                                               $"Please remove usb\n Usb Message: " +
                                                               $"{result.FirstError.Description}");
        } else {
            this._hubContext.Clients.All.OnUsbDisconnect("Usb Disconnected");
        }
        /*this._stateMachine.FireAsync(ControllerTrigger.NotConnected);*/
        return Task.FromResult(result);
    }

    public async Task<ErrorOr<Success>> Stop() {
        var result = await this.Disconnect();
        await this._cancellationTokenSource.CancelAsync();
        return result;
        /*if (!result.IsError) {
            return result;
        } else {
            /*string message = "Error: Usb failed to disconnect.  Please remove usb";
            message += $"\n Usb Message: {result.FirstError.Description}";
            return Task.FromResult<ErrorOr<Success>>(Error.Failure(description:message));#1#
        }*/
    }
    
    private async Task StartReaderAsync(CancellationToken token) {
        while (await this._channelReader.WaitToReadAsync(token)) {
            while (this._channelReader.TryRead(out var message)) {
                await this._sender.Send(new StationMessage() { Message = message }, token);
            }
        }
    }
    
    private void UsbControllerOnUsbStateChangedHandler(Object? sender, ConnectionStatusChangedEventArgs e) {
        if (e.Connected) {
            this._hubContext.Clients.All.OnUsbConnect("Usb Connected");
            /*if(CanFire(ControllerTrigger.Connected)) {
                this._stateMachine.Fire(ControllerTrigger.Connected);
            }*/
        } else {
            if (CanFire(ControllerTrigger.NotConnected)) {
                //this._stateMachine.Fire(ControllerTrigger.NotConnected);
            }
            if (e.ConnectionEventType == ConnectionEventType.DisconnectWithRetry) {
                this._hubContext.Clients.All.OnUsbDisconnect("Error: Usb Disconnected. Please check usb cable \n" +
                                                             "The system will reconnect once the cable is plugged back in.");
            } else {
                this._hubContext.Clients.All.OnUsbDisconnect("Usb Disconnected,to reconnect please press the connect button");
            }
        }
    }
    
    public Task<ErrorOr<Success>> Send<TPacket>(StationMsgPrefix prefix,TPacket packet) where TPacket:IPacket {
        MessagePacket<TPacket> msgPacket = new MessagePacket<TPacket>() {
            Prefix = prefix,
            Packet = packet
        };
        var result = this._usbController.Send(msgPacket);
        if (!result.IsError) {
            this._logger.LogInformation("Msg Sent of type {ArduinoMsgPrefix.Name}",msgPacket.Prefix.Name);
            return Task.FromResult(result);
        } else {
            var message = $"Failed to send {msgPacket.Prefix.Name}, Error {result.FirstError.Description}";
            this._logger.LogError(message);
            return Task.FromResult<ErrorOr<Success>>(Error.Failure(description:message));
        }
    }

    private bool CanFire(ControllerTrigger trigger) {
        //return this._stateMachine.CanFire(trigger);
        return true;
    }

    private async Task BuildStateMachine() {
        /*this._stateMachine.Configure(ControllerState.NotDefined)
            .Permit(ControllerTrigger.Startup, ControllerState.Startup);
        
        this._stateMachine.Configure(ControllerState.Startup)
            .OnEntryFromAsync(ControllerTrigger.Reset,this.ConnectUsb)
            .Permit(ControllerTrigger.Connected, ControllerState.Connected)
            .Permit(ControllerTrigger.NotConnected, ControllerState.NotConnected)
            .OnExit(()=>this._stateData.IsConfigured=true);

        this._stateMachine.Configure(ControllerState.TryConnect)
            .OnEntryAsync(this.ConnectUsb)
            .Permit(ControllerTrigger.NotConnected, ControllerState.NotConnected)
            .Permit(ControllerTrigger.Connected, ControllerState.Connected);

        this._stateMachine.Configure(ControllerState.TryDisconnect)
            .OnEntryAsync(this.Disconnect)
            .Permit(ControllerTrigger.NotConnected, ControllerState.NotConnected);
        
        this._stateMachine.Configure(ControllerState.NotConnected)
            .OnEntry(() => {
                this._retryTimer.AutoReset = false;
                this._retryTimer.Start();
            })
            .Permit(ControllerTrigger.Connect, ControllerState.TryConnect)
            .Permit(ControllerTrigger.Reset, ControllerState.Startup)
            .Permit(ControllerTrigger.Connected,ControllerState.Connected)
            .Ignore(ControllerTrigger.NotConnected)
            .Ignore(ControllerTrigger.Disconnect)
            .Ignore(ControllerTrigger.StartTest)
            .Ignore(ControllerTrigger.PauseTest)
            .Ignore(ControllerTrigger.ContinueTest)
            .OnExit(() => {
                this._retryTimer.Stop();
            });

        this._stateMachine.Configure(ControllerState.Connected)
            .InitialTransition(ControllerState.Idle);
        
        this._stateMachine.Configure(ControllerState.Idle)
            .SubstateOf(ControllerState.Connected)
            .Permit(ControllerTrigger.StartTest, ControllerState.Running)
            .Permit(ControllerTrigger.Disconnect, ControllerState.NotConnected)
            .Permit(ControllerTrigger.NotConnected,ControllerState.NotConnected)
            .Ignore(ControllerTrigger.PauseTest)
            .Ignore(ControllerTrigger.ContinueTest);

        this._stateMachine.Configure(ControllerState.Running)
            .SubstateOf(ControllerState.Connected)
            .Permit(ControllerTrigger.PauseTest, ControllerState.Paused)
            .Permit(ControllerTrigger.NotConnected,ControllerState.NotConnected)
            .Permit(ControllerTrigger.Disconnect, ControllerState.NotConnected);

        this._stateMachine.Configure(ControllerState.Paused)
            .SubstateOf(ControllerState.Connected)
            .Permit(ControllerTrigger.ContinueTest, ControllerState.Running)
            .Permit(ControllerTrigger.NotConnected,ControllerState.NotConnected)
            .Permit(ControllerTrigger.Disconnect, ControllerState.NotConnected);

        this._stateMachine.Configure(ControllerState.Error)
            .Permit(ControllerTrigger.Reset, ControllerState.Startup);
        
        this._stateMachine.OnUnhandledTriggerAsync((state, trigger) => {
            this._logger.LogError("Unhandled Trigger {Trigger} in State {State}", trigger, state);
            return Task.CompletedTask;
        });
        
        this._stateMachine.OnTransitioned(this.OnTransition);
        await this._stateMachine.FireAsync(ControllerTrigger.Startup);*/
    }
    
    private void OnTransition(StateMachine<ControllerState, ControllerTrigger>.Transition transition) {
        this._logger.LogInformation($"Transition: {transition.Source} -> {transition.Destination} : {transition.Trigger}");
    }
    
    public void Dispose() {
        this._usbController.Dispose();
    }
}
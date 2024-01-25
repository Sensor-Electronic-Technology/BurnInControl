using AsyncAwaitBestPractices;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Channels;
using FileMode=System.IO.FileMode;
namespace BurnIn.Shared.Controller;

public record ControllerResult(bool Success, string? Message) {
    public bool Success { get; set; } = Success;
    public string? Message { get; set; } = Message;
}

public class StationController:IDisposable {
    private readonly UsbController _usbController;
    private readonly ILogger<StationController> _logger;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly ChannelReader<string> _channelReader;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly FirmwareVersionService _firmwareService;
    private readonly BurnInTestService _testService;
    //private readonly GitHubClient _github;
    private string _latestVersion=string.Empty;
    private bool _initMessageSent = false;
    private bool _receivedVersion = false;

    public StationController(IHubContext<StationHub,
            IStationHub> hubContext, 
            UsbController usbController,
            ChannelReader<string> channelReader,
            FirmwareVersionService firmwareService,
            BurnInTestService testService,
            ILogger<StationController> logger) {
        this._logger = logger;
        this._channelReader = channelReader;
        this._usbController = usbController;
        this._hubContext = hubContext;
        this._usbController.UsbUnPlogHandler += this.UsbUnplugHandler;
        this._firmwareService = firmwareService;
        this._testService = testService;
        //this._github=new GitHubClient(new ProductHeaderValue("Sensor-Electronic-Technology"));
    }

    public Task Start() {
        this._firmwareService.GetLatestVersion().SafeFireAndForget();
        return this.ConnectUsb();
    }
    
    public Task<ControllerResult> ConnectUsb() {
        if (!this._usbController.Connected) {
            var result=this._usbController.Connect();
            if (result.State == UsbState.Connected) {
                this.StartReaderAsync(this._cancellationTokenSource.Token)
                    .SafeFireAndForget(e => {
                        this._logger.LogCritical("Channel read failed");
                    });
                this._logger.LogInformation("Usb connected");
                return Task.FromResult(new ControllerResult(true, "Usb Connected"));
            } else {
                this._logger.LogCritical($"Usb failed to connect.  Error\n {result.Message}");
                return Task.FromResult(new ControllerResult(false, result.Message));
            }
        } else {
            this._logger.LogWarning($"Usb already connected");
            return Task.FromResult(new ControllerResult(false,"Usb already connected"));
        }
    }

    public Task<ControllerResult> Disconnect() {
        this._usbController.Disconnect();
        return Task.FromResult(new ControllerResult(true,"result.Message"));
    }

    public Task<ControllerResult> Stop() {
        var result=this._usbController.Disconnect();
        this._cancellationTokenSource.Cancel();
        if (result.State == UsbState.Disconnected) {
            return Task.FromResult(new ControllerResult(true,result.Message)); 
        } else {
            string message = "Error: Usb failed to disconnect.  Please remove usb";
            message += $"\n Usb Message: {result.Message}";
            return Task.FromResult(new ControllerResult(false,message));
        }
    }
    
    private async Task StartReaderAsync(CancellationToken token) {
        while (await this._channelReader.WaitToReadAsync(token)) {
            while (this._channelReader.TryRead(out var message)) {
                await this.HandleMessagePacket(message);
            }
        }
    }

    private void UsbUnplugHandler(object? sender,EventArgs args) {
        Console.WriteLine("Usb was disconnected");
        this._hubContext.Clients.All.OnUsbDisconnect(true);
    }
    
    public Task<ControllerResult> SendV2<TPacket>(ArduinoMsgPrefix prefix,TPacket packet) where TPacket:IPacket {
        MessagePacketV2<TPacket> msgPacket = new MessagePacketV2<TPacket>() {
            Prefix = prefix,
            Packet = packet
        };
        var result = this._usbController.SendV2(msgPacket);
        if (result.Success) {
            this._logger.LogInformation("Msg Sent of type {ArduinoMsgPrefix.Name}",msgPacket.Prefix.Name);
            return Task.FromResult(new ControllerResult(result.Success,"Message Sent"));
        } else {
            this._logger.LogError("Failed to send {ArduinoMsgPrefix.Name}, Error {Error}",msgPacket.Prefix.Name,result.Message);
            return Task.FromResult(new ControllerResult(result.Success, $"Failed to send message.  Internal Error: {result.Message}"));
        }
    }
    
    private Task HandleMessagePacket(string message) {
        try {
            var doc=JsonSerializer.Deserialize<JsonDocument>(message);
            var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
            if (!string.IsNullOrEmpty(prefixValue)) {
                var prefix=ArduinoMsgPrefix.FromValue(prefixValue);
                if (prefix != null) {
                    var packetElem=doc.RootElement.GetProperty("Packet");
                    prefix.When(ArduinoMsgPrefix.DataPrefix).Then(()=>this.HandleData(packetElem))
                        .When(ArduinoMsgPrefix.MessagePrefix).Then(()=>this.HandleMessage(packetElem,false))
                        .When(ArduinoMsgPrefix.InitMessage).Then(()=>this.HandleMessage(packetElem,true))
                        .When(ArduinoMsgPrefix.IdRequest).Then(()=>this.HandleIdChanged(packetElem))
                        .When(ArduinoMsgPrefix.VersionRequest).Then(()=>this.HandleReceiveVersion(packetElem));
                }
            }
        } catch {
            this._logger.LogWarning($"Message had errors.  Message: {message}");
        }
        return Task.CompletedTask;
    }

    private void HandleData(JsonElement element) {
        try {
            var serialData=element.Deserialize<StationSerialData>();
            if (serialData != null) {
                if (!this._testService.IsRunning && serialData.Running) {
                    var result = this._testService.Log(serialData);
                    if (result is SuccessResult successResult) {
                        this._hubContext.Clients.All.OnTestStarted(successResult.Message);
                    }else if (result is ErrorResult errorResult) {
                        this._hubContext.Clients.All.OnTestStartedFailed($"Failed start logging test.  " +
                                                                         $"Message {errorResult.Message}");
                    }
                }
                this._hubContext.Clients.All.OnSerialCom(serialData).SafeFireAndForget();
                
            }
        } catch(Exception e) {
            this._logger.LogWarning("Failed to deserialize station data");
        }
    }

    private void HandleMessage(JsonElement element,bool isInit) {
        if (isInit) {
            this._initMessageSent = true;
        } else {
            this._initMessageSent = false;
            if (!this._receivedVersion) {
                
            }
        }
        var message=element.GetProperty("Message").ToString();
        this._hubContext.Clients.All.OnSerialComMessage(message).SafeFireAndForget();
    }

    private void HandleIdChanged(JsonElement element) {
        var id = element.GetString();
        this._hubContext.Clients.All.OnIdChanged(id);
    }

    private void HandleReceiveVersion(JsonElement element) {
        try {
            var version = element.GetString();
            if (!string.IsNullOrEmpty(version)) {
                this._receivedVersion = true;
                var status=this._firmwareService.CheckNewerVersion(version);
                if (status.UpdateReady) {
                    this._usbController.Disconnect();
                    //this._firmwareService.
                }
            } else {
                this._logger.LogInformation("Failed to check firmware version. Version string was null or empty");
            }

        } catch(Exception e) {
            
        }

    }

    public void Dispose() {
        this._usbController.Dispose();
    }
}
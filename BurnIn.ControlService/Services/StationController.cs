﻿using AsyncAwaitBestPractices;
using BurnIn.ControlService.Hubs;
using BurnIn.Shared;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Threading.Channels;
namespace BurnIn.ControlService.Services;

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

    public StationController(IHubContext<StationHub, IStationHub> hubContext, 
            UsbController usbController,
            ChannelReader<string> channelReader,
            FirmwareVersionService firmwareService,
            BurnInTestService testService,
            ILogger<StationController> logger) {
        this._logger = logger;
        this._channelReader = channelReader;
        this._usbController = usbController;
        this._hubContext = hubContext;
        this._usbController.UsbUnPlugHandler += this.UsbUnplugHandler;
        this._firmwareService = firmwareService;
        this._testService = testService;
    }

    public Task Start() {
        return this.ConnectUsb();
    }
    
    public Task<Result> ConnectUsb() {
        if (!this._usbController.Connected) {
            var result=this._usbController.Connect();
            if (result.IsSuccess) {
                this.StartReaderAsync(this._cancellationTokenSource.Token)
                    .SafeFireAndForget(e => {
                        this._logger.LogCritical("Channel read failed");
                    });
                this._logger.LogInformation("Usb connected");
                return Task.FromResult(ResultFactory.Success("Usb Connected"));
            } else {
                this._logger.LogCritical($"Usb failed to connect.  Error\n {result.Message}");
                return Task.FromResult(ResultFactory.Error($"Usb failed to connect.  Error\n {result.Message}"));
            }
        } else {
            this._logger.LogWarning($"Usb already connected");
            return Task.FromResult(ResultFactory.Success("Usb already connected"));
        }
    }

    public Task<Result> Disconnect() {
        var result=this._usbController.Disconnect();
        return Task.FromResult(result);
    }

    public Task<Result> Stop() {
        var result=this._usbController.Stop();
        this._cancellationTokenSource.Cancel();
        if (result.IsSuccess) {
            return Task.FromResult(result);
        } else {
            string message = "Error: Usb failed to disconnect.  Please remove usb";
            message += $"\n Usb Message: {result.Error}";
            return Task.FromResult(ResultFactory.Error(message));
        }
    }

    public async Task<Result> CheckForUpdate() {
        await this._firmwareService.GetLatestVersion();
        var result=this._usbController.RequestFirmwareVersion();
        if (result.IsSuccess) {
            return result;
        } else {
            return ResultFactory.Error($"Error Requesting Version " +
                                       $"Error Message: {result.Message}");
        }
    }

    public async Task UpdateFirmware() {
        var result=this._usbController.Disconnect();
        if (result.IsSuccess) {
            await this._firmwareService.DownloadFirmwareUpdate();
            this._firmwareService.UploadFirmwareUpdate();
            this._usbController.Connect();
            await this._hubContext.Clients.All.OnFirmwareUpdated(true, this._firmwareService.Version, "Updated");
        }
        await this._hubContext.Clients.All.OnFirmwareUpdated(false, this._firmwareService.Version, "Update Failed");
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
    
    public Task<Result> Send<TPacket>(ArduinoMsgPrefix prefix,TPacket packet) where TPacket:IPacket {
        MessagePacketV2<TPacket> msgPacket = new MessagePacketV2<TPacket>() {
            Prefix = prefix,
            Packet = packet
        };
        var result = this._usbController.Send(msgPacket);
        if (result.IsSuccess) {
            this._logger.LogInformation("Msg Sent of type {ArduinoMsgPrefix.Name}",msgPacket.Prefix.Name);
            return Task.FromResult(result);
        } else {
            this._logger.LogError("Failed to send {ArduinoMsgPrefix.Name}, Error {Error}",msgPacket.Prefix.Name,result.Message);
            return Task.FromResult(ResultFactory.Error($"Failed to send message.  Internal Error: {result.Message}"));
        }
    }
    
    private Task HandleMessagePacket(string message) {
        try {
            if (message.Contains("Prefix")) {
                var doc=JsonSerializer.Deserialize<JsonDocument>(message);
                var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
                if (!string.IsNullOrEmpty(prefixValue)) {
                    var prefix=ArduinoMsgPrefix.FromValue(prefixValue);
                    if (prefix != null) {
                        var packetElem=doc.RootElement.GetProperty("Packet");
                        prefix.When(ArduinoMsgPrefix.DataPrefix).Then(() => this.HandleData(packetElem))
                            .When(ArduinoMsgPrefix.MessagePrefix).Then(() => this.HandleMessage(packetElem, false))
                            .When(ArduinoMsgPrefix.InitMessage).Then(() => this.HandleMessage(packetElem, true))
                            .When(ArduinoMsgPrefix.IdRequest).Then(() => this.HandleIdChanged(packetElem))
                            .When(ArduinoMsgPrefix.VersionRequest).Then(()=>this.HandleVersionRequest(packetElem));
                    }
                }
            } else {
                this._hubContext.Clients.All.OnSerialComMessage(message);
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
                    if (result.IsSuccess) {
                        this._hubContext.Clients.All.OnTestStarted();
                    }else{
                        this._hubContext.Clients.All.OnTestStartedFailed($"Failed start logging test. " +
                                                                         $"Message {result.Error}");
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
    
    private void HandleVersionRequest(JsonElement element) {
        try {
            var version = element.GetString();
            if (!string.IsNullOrEmpty(version)) {
                this._receivedVersion = true;
                FirmwareUpdateStatus status = this._firmwareService.CheckNewerVersion(version);
                this._hubContext.Clients.All.OnUpdateChecked(status);
            } else {
                this._hubContext.Clients.All.OnUpdateChecked(new FirmwareUpdateStatus() {
                    Message = "Failed to check firmware version. Version string was null or empty",
                    UpdateReady = false,
                    Type=UpdateType.None
                });
                this._logger.LogError("Failed to check firmware version. Version string was null or empty");
            }
        } catch(Exception e) {
            this._hubContext.Clients.All.OnUpdateChecked(new FirmwareUpdateStatus() {
                Message =$"Update check failed Exception: {e.Message}",
                UpdateReady = false,
                Type=UpdateType.None
            });
            this._logger.LogError("Update check failed Exception: {Error}",e.Message);
        }
    }
    
    public void Dispose() {
        this._usbController.Dispose();
    }
}
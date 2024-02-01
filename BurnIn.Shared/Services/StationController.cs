using AsyncAwaitBestPractices;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
namespace BurnIn.Shared.Services;
public class StationController:IDisposable {
    private readonly UsbController _usbController;
    private readonly ILogger<StationController> _logger;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    private readonly ChannelReader<string> _channelReader;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly FirmwareVersionService _firmwareService;
    private readonly BurnInTestService _testService;
    private readonly MessageHandler _messageHandler;

    public StationController(IHubContext<StationHub, IStationHub> hubContext, 
            UsbController usbController,
            ChannelReader<string> channelReader,
            FirmwareVersionService firmwareService,
            BurnInTestService testService,
            MessageHandler messageHandler,
            ILogger<StationController> logger) {
        this._logger = logger;
        this._channelReader = channelReader;
        this._usbController = usbController;
        this._hubContext = hubContext;
        this._usbController.UsbUnPlugHandler += this.UsbUnplugHandler;
        this._firmwareService = firmwareService;
        this._testService = testService;
        this._messageHandler = messageHandler;
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
                await this._messageHandler.Handle(message);
            }
        }
    }
    
    private void UsbUnplugHandler(object? sender,EventArgs args) {
        this._logger.LogWarning("Usb Disconnected");
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
    
    public void Dispose() {
        this._usbController.Dispose();
    }
}
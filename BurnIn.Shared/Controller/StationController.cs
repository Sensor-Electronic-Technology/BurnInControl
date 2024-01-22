using AsyncAwaitBestPractices;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Channels;
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
    private bool _running = false;
    

    public StationController(IHubContext<StationHub,
            IStationHub> hubContext, 
            UsbController usbController,
            ChannelReader<string> channelReader,
            ILogger<StationController> logger) {
        this._logger = logger;
        this._channelReader = channelReader;
        this._usbController = usbController;
        this._hubContext = hubContext;
        this._usbController.UsbUnPlogHandler += this.UsbUnplugHandler;
    }

    public Task Start() {
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
                await this._hubContext.Clients.All.OnSerialComMessage(message);
            }
        }
    }

    private void UsbUnplugHandler(object? sender,EventArgs args) {
        Console.WriteLine("Usb was disconnected");
        this._hubContext.Clients.All.OnUsbDisconnect(true);
    }

    public Task<ControllerResult> Send(MessagePacket packet) {
        var result=this._usbController.Send(packet);
        if (result.Success) {
            return Task.FromResult(new ControllerResult(true, "Send Success"));
        } else {
            return Task.FromResult(new ControllerResult(false, result.Message));
        }
    }

    public void Dispose() {
        this._usbController.Dispose();
    }
}
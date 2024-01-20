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

public class StationController {
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
    }

    public Task Start() {
        if (this._usbController.Connect()) {
            this._usbController.StartReadingAsync(this._cancellationTokenSource.Token)
                .SafeFireAndForget(e => {
                    this._logger.LogCritical("Usb read failed");
                });
            this.StartReaderAsync(this._cancellationTokenSource.Token)
                .SafeFireAndForget(e => {
                    this._logger.LogCritical("Channel read failed");
                });
            /*return Task.FromResult(new ControllerResult(
            true,
            "result.Message"));*/
        }
        return Task.CompletedTask;
    }
    
    public Task<ControllerResult> ConnectUsb() {
        /*if (this._usbController.Connect()) {
            this._usbController.StartReadingAsync(this._cancellationTokenSource.Token)
                .SafeFireAndForget(e => {
                    this._logger.LogCritical("Usb read failed");
                });
            this.StartReaderAsync(this._cancellationTokenSource.Token)
                .SafeFireAndForget(e => {
                    this._logger.LogCritical("Channel read failed");
                });
            return Task.FromResult(new ControllerResult(
                true,
                "result.Message"));
        }*/
        return Task.FromResult(new ControllerResult(
            true,
            "Already Connected"));
    }

    public Task<ControllerResult> Disconnect() {
        this._cancellationTokenSource.Cancel();
        return Task.FromResult(new ControllerResult(true,"result.Message"));
    }

    private async Task StartReaderAsync(CancellationToken token) {
        await foreach (var message in this._channelReader.ReadAllAsync(token)) {
            await this._hubContext.Clients.All.OnSerialComMessage(message);
            Console.WriteLine(message);
        }
    }

    public Task<ControllerResult> ExecuteCommand(ArduinoCommand command) {
        MessagePacket msgPacket = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.CommandPrefix.Value,
            Packet = command.Value
        };
        var output = JsonSerializer.Serialize<MessagePacket>(msgPacket,
        new JsonSerializerOptions(){WriteIndented = false});
        this._usbController.Send(msgPacket);
        return Task.FromResult(new ControllerResult(true, ""));
    }

    /*public Task<ControllerResult> UpdateArduinoSettings(string command) {
        var usbResult=this._usbController.WriteCommand(command);
        if (usbResult.Success) {
            return Task.FromResult(new ControllerResult(true,"Settings Sent"));
        } else {
            return Task.FromResult(new ControllerResult(false,"Error: Settings Failed To Upload "));
        }
    }*/
    
}
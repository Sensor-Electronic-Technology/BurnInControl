using AsyncAwaitBestPractices;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Services;
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
                //await this._hubContext.Clients.All.OnSerialComMessage(message);
                await this.HandleMessagePacket(message);
                //this._logger.LogInformation(message);
                /*this.HandleMessagePacket(message).SafeFireAndForget(e => {
                    if (e.InnerException != null) {
                        this._logger.LogError("Error while parsing msg packet.  " +
                                              "Exception: {Message} \n Inner: {InnerMessage}",e.Message,e.InnerException.Message);
                    } else {
                        this._logger.LogError("Error while parsing msg packet. Exception: {Message}",e.Message);
                    }
                });*/
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
    
    public Task HandleMessagePacket(string message) {
        
        try {
            var doc=JsonSerializer.Deserialize<JsonDocument>(message);
            var prefixValue=doc.RootElement.GetProperty("Prefix").ToString();
            if (!string.IsNullOrEmpty(prefixValue)) {
                var prefix=ArduinoMsgPrefix.FromValue(prefixValue);
                if (prefix != null) {
                    var packetElem=doc.RootElement.GetProperty("Packet");
                    prefix.When(ArduinoMsgPrefix.DataPrefix).Then(()=>this.HandleData(packetElem))
                        .When(ArduinoMsgPrefix.MessagePrefix).Then(()=>this.HandleMessage(packetElem))
                        .When(ArduinoMsgPrefix.IdRequest).Then(()=>this.HandleIdChanged(packetElem));
                }
            }
        } catch {
            Console.WriteLine($"Message had errors.  Message: {message}");
        }
        return Task.CompletedTask;
    }

    private void HandleData(JsonElement element) {
        var serialData=element.Deserialize<StationSerialData>();
        this._hubContext.Clients.All.OnSerialCom(serialData).SafeFireAndForget();
    }

    private void HandleMessage(JsonElement element) {
        var message=element.GetProperty("Message").ToString();
        this._hubContext.Clients.All.OnSerialComMessage(message).SafeFireAndForget();
    }

    private void HandleIdChanged(JsonElement element) {
        var id = element.GetString();
        this._hubContext.Clients.All.OnIdChanged(id);
    }

    public void Dispose() {
        this._usbController.Dispose();
    }
}
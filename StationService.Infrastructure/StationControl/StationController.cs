using AsyncAwaitBestPractices;
using BurnInControl.Application.ProcessSerial.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.Shared.Hubs;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SerialPortLib;
using StationService.Infrastructure.Hub;
using StationService.Infrastructure.SerialCom;
using System.Threading.Channels;
namespace StationService.Infrastructure.StationControl;

public class StationController:IStationController,IDisposable {
    private readonly UsbController _usbController;
    private readonly ILogger<StationController> _logger;
    private readonly ChannelReader<string> _channelReader;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ISender _sender;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
    
    public StationController(UsbController usbController,
        ChannelReader<string> channelReader,
        ISender sender,
        IHubContext<StationHub, IStationHub> hubContext,
        ILogger<StationController> logger) {
        this._logger = logger;
        this._channelReader = channelReader;
        this._usbController = usbController;
        this._usbController.UsbStateChangedHandler+= UsbControllerOnUsbStateChangedHandler;
        this._sender = sender;
        this._hubContext = hubContext;
    }


    public Task Start() {
        return this.ConnectUsb();
    }
    
    public Task<ErrorOr<Success>> ConnectUsb() {
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
            return Task.FromResult<ErrorOr<Success>>(result.Value);
        } else {
            this._hubContext.Clients.All.OnUsbConnect("Usb Connected");
            return Task.FromResult<ErrorOr<Success>>(result.Value);
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
        return Task.FromResult(result);
    }

    public Task<ErrorOr<Success>> Stop() {
        var result=this._usbController.Stop();
        this._cancellationTokenSource.Cancel();
        if (!result.IsError) {
            return Task.FromResult(result);
        } else {
            string message = "Error: Usb failed to disconnect.  Please remove usb";
            message += $"\n Usb Message: {result.FirstError.Description}";
            return Task.FromResult<ErrorOr<Success>>(Error.Failure(description:message));
        }
    }
    
    private async Task StartReaderAsync(CancellationToken token) {
        while (await this._channelReader.WaitToReadAsync(token)) {
            while (this._channelReader.TryRead(out var message)) {
                await this._sender.Send(new StationMessage() { Message = message }, token);
            }
        }
    }
    
    private void UsbControllerOnUsbStateChangedHandler(Object? sender, ConnectionStatusChangedEventArgs e) {
        this._logger.LogWarning("Usb Disconnected");
        if (e.Connected) {
            this._hubContext.Clients.All.OnUsbConnect("Usb Connected");
        } else {
            this._hubContext.Clients.All.OnUsbDisconnect("Error: Usb Disconnected.  Please check usb cable \n" +
                                                         "The system will reconnect once the cable is plugged back in.");
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
    
    public void Dispose() {
        this._usbController.Dispose();
    }
}
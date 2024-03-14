using AsyncAwaitBestPractices;
using BurnInControl.Application.ProcessSerial.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using StationService.Infrastructure.SerialCom;
using System.Threading.Channels;
namespace StationService.Infrastructure.StationControl;

public class StationController:IStationController,IDisposable {
    private readonly UsbController _usbController;
    private readonly ILogger<StationController> _logger;
    private readonly ChannelReader<string> _channelReader;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ISender _sender;
    //private readonly StationMessageHandler _stationMessageHandler;
    
    public StationController(UsbController usbController,
        ChannelReader<string> channelReader,
        ISender sender,
        ILogger<StationController> logger) {
        this._logger = logger;
        this._channelReader = channelReader;
        this._usbController = usbController;
        this._usbController.UsbUnPlugHandler += this.UsbUnplugHandler;
        this._sender = sender;
    }
    
    public Task Start() {
        return this.ConnectUsb();
    }
    
    public Task<ErrorOr<Success>> ConnectUsb() {
        if (!this._usbController.Connected) {
            var result=this._usbController.Connect();
            if (!result.IsError) {
                this.StartReaderAsync(this._cancellationTokenSource.Token)
                    .SafeFireAndForget(e => {
                        this._logger.LogWarning($"Channel read failed. Exception: \n {e.Message}");
                    });
                return Task.FromResult<ErrorOr<Success>>(result.Value);
            } else {
                return Task.FromResult<ErrorOr<Success>>(result.Value);
            }
        } else {
            return Task.FromResult<ErrorOr<Success>>(Error.Conflict(description:"Usb already connected"));
        }
    }

    public Task<ErrorOr<Success>> Disconnect() {
        var result=this._usbController.Disconnect();
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
                //this._stationMessageHandler.Handle(message, token);
                await this._sender.Send(new StationMessage() { Message = message }, token);
            }
        }
    }
    
    private void UsbUnplugHandler(object? sender,EventArgs args) {
        this._logger.LogWarning("Usb Disconnected");
        //this._hubContext.Clients.All.OnUsbDisconnect(true);
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
using AsyncAwaitBestPractices;
using BurnInControl.Application.ProcessSerial.Messages;
using BurnInControl.Application.StationControl.Interfaces;
using BurnInControl.Shared.AppSettings;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using BurnInControl.HubDefinitions.Hubs;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SerialPortLib;
using StationService.Infrastructure.Hub;
using StationService.Infrastructure.SerialCom;
using System.Threading.Channels;
using BurnInControl.Shared;

namespace StationService.Infrastructure.StationControl;

public class StationController:IStationController,IDisposable {
    private readonly UsbController _usbController;
    private readonly ILogger<StationController> _logger;
    private readonly ChannelReader<string> _channelReader;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly ISender _sender;
    private readonly IHubContext<StationHub, IStationHub> _hubContext;
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
    }

    public async Task Start() {
        await this.ConnectUsb();
    }
    
    public Task GetConnectionStatus() {
       return this._hubContext.Clients.All.OnStationConnection(this._usbController.Connected);
    }

    public async Task<ErrorOr<Success>> ConnectUsb() {
        var result=this._usbController.Connect();
        if (!result.IsError) {
            this._cancellationTokenSource = new CancellationTokenSource();
            this.StartReaderAsync(this._cancellationTokenSource.Token)
                .SafeFireAndForget(e => {
                    this._hubContext.Clients.All.OnUsbConnectFailed($"Channel read error: Exception: \n{e.ToErrorMessage()}");
                    this._logger.LogError($"Channel read error: Exception: \n{e.ToErrorMessage()}");
                });
            return result;
        } else {
            this._hubContext.Clients.All.OnUsbConnectFailed($"Usb failed to connect.  " +
                                                            $"Please check usb cable" +
                                                            $"\n Usb Message: {result.FirstError.Description})")
                .SafeFireAndForget();
            return result;
        }
    }

    public async Task<ErrorOr<Success>> Disconnect() {
        var result=this._usbController.Disconnect();
        await this._cancellationTokenSource.CancelAsync();
        if (result.IsError) {
            await this._hubContext.Clients.All.OnUsbDisconnectFailed($"Usb failed to disconnect.  " +
                                                               $"Please remove usb\n Usb Message: " +
                                                               $"{result.FirstError.Description}");
        } else {
            await this._hubContext.Clients.All.OnUsbDisconnect("Usb Disconnected");
        }
        return result;
    }

    public async Task<ErrorOr<Success>> Stop() {
        //var result = await this.Disconnect();
        var result=this._usbController.Stop();
        await this._cancellationTokenSource.CancelAsync();
        return result;
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
        } else {
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
            this._logger.LogInformation("Msg Sent of type {MessageType}",msgPacket.Prefix.Value);
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
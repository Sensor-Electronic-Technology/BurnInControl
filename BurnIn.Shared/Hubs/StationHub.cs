using BurnIn.Shared.Controller;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
namespace BurnIn.Shared.Hubs;

public class StationHub:Hub<IStationHub> {

    private readonly StationController _controller;

    public StationHub(StationController controller) {
        this._controller = controller;
    }
    
    public Task<ControllerResult> ConnectUsb() {
        return this._controller.ConnectUsb();
    }

    public Task<ControllerResult> DisconnectUsb() {
        return this._controller.Disconnect();
    }

    public Task<ControllerResult> Send(MessagePacket packet) {
        return this._controller.Send(packet);
    }
    
    public Task<ControllerResult> SendCommand(ArduinoCommand command) {
        MessagePacket msg = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.CommandPrefix,
            Packet = command.Value
        };
        Console.WriteLine($"Received Command: {command.Name}");
        return this._controller.Send(msg);
    }

    public Task<ControllerResult> SendProbeConfig(ProbeControllerConfig packet) {
        MessagePacket msg = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.ProbeConfigPrefix,
            Packet = packet
        };
        return this._controller.Send(msg);
    }
    
    public Task<ControllerResult> SendHeaterConfig(HeaterControllerConfig packet) {
        MessagePacket msg = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.HeaterConfigPrefix,
            Packet = packet
        };
        return this._controller.Send(msg);
    }
    
    public Task<ControllerResult> SendStationConfig(StationConfiguration packet) {
        MessagePacket msg = new MessagePacket() {
            Prefix = ArduinoMsgPrefix.StationConfigPrefix,
            Packet = packet
        };
        return this._controller.Send(msg);
    }
}
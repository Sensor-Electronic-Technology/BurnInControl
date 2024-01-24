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
    
    public Task<ControllerResult> SendCommand(ArduinoCommand command) {
        return this._controller.SendV2(ArduinoMsgPrefix.CommandPrefix,command);
    }

    public Task<ControllerResult> SendId(string newId) {
        return this._controller.SendV2(ArduinoMsgPrefix.IdReceive,new StationIdPacket() { StationId = newId });
    }
    
    public Task<ControllerResult> RequestId() {
        return this._controller.SendV2(ArduinoMsgPrefix.IdRequest,ArduinoMsgPrefix.IdRequest);
    }
    
    public Task<ControllerResult> SendVersion(string newVersion) {
        return this._controller.SendV2(ArduinoMsgPrefix.VersionReceive,new StationVersionPacket() { Version = newVersion });
    }
    
    public Task<ControllerResult> RequestVersion() {
        return this._controller.SendV2(ArduinoMsgPrefix.VersionRequest,ArduinoMsgPrefix.VersionRequest);
    }

    public Task<ControllerResult> SendProbeConfig(ProbeControllerConfig packet) {
        return this._controller.SendV2(ArduinoMsgPrefix.ProbeConfigPrefix,packet);
    }
    
    public Task<ControllerResult> SendHeaterConfig(HeaterControllerConfig packet) {
        return this._controller.SendV2(ArduinoMsgPrefix.HeaterConfigPrefix,packet);
    }
    
    public Task<ControllerResult> SendStationConfig(StationConfiguration packet) {
        return this._controller.SendV2(ArduinoMsgPrefix.StationConfigPrefix,packet);
    }
}
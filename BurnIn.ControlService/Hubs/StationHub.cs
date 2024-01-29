using BurnIn.ControlService.Services;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using Microsoft.AspNetCore.SignalR;
namespace BurnIn.ControlService.Hubs;

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

    public Task<ControllerResult> SendStartTest() {
        return this._controller.Send(ArduinoMsgPrefix.CommandPrefix, ArduinoCommand.Start);
    }
    
    public Task<ControllerResult> SendCommand(ArduinoCommand command) {
        return this._controller.Send(ArduinoMsgPrefix.CommandPrefix,command);
    }

    public Task<ControllerResult> SendId(string newId) {
        return this._controller.Send(ArduinoMsgPrefix.IdReceive,new StationIdPacket() { StationId = newId });
    }
    
    public Task<ControllerResult> RequestId() {
        return this._controller.Send(ArduinoMsgPrefix.IdRequest,ArduinoMsgPrefix.IdRequest);
    }

    public async Task<ControllerResult> CheckForUpdate() {
        return await this._controller.CheckForUpdate();
    }

    public async Task UpdateFirmware() {
        await this._controller.UpdateFirmware();
    }

    public Task<ControllerResult> SendProbeConfig(ProbeControllerConfig packet) {
        return this._controller.Send(ArduinoMsgPrefix.ProbeConfigPrefix,packet);
    }
    
    public Task<ControllerResult> SendHeaterConfig(HeaterControllerConfig packet) {
        return this._controller.Send(ArduinoMsgPrefix.HeaterConfigPrefix,packet);
    }
    
    public Task<ControllerResult> SendStationConfig(StationConfiguration packet) {
        return this._controller.Send(ArduinoMsgPrefix.StationConfigPrefix,packet);
    }

    public Task<ControllerResult> SendFirmwareVersion(string newVersion) {
        return this._controller.Send(ArduinoMsgPrefix.VersionReceive,new StationVersionPacket() { Version = newVersion });
    }
}
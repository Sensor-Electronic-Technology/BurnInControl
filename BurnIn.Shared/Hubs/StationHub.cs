using BurnIn.Shared.Models;
using BurnIn.Shared.Models.BurnInStationData;
using BurnIn.Shared.Models.Configurations;
using BurnIn.Shared.Services;
using Microsoft.AspNetCore.SignalR;
namespace BurnIn.Shared.Hubs;

public class StationHub:Hub<IStationHub> {
    private readonly StationController _controller;
    private readonly BurnInTestService _testService;
    public StationHub(StationController controller,BurnInTestService testService) {
        this._controller = controller;
        this._testService = testService;
    }
    
    public Task ConnectUsb() {
        return this._controller.ConnectUsb();
    }
    
    public Task DisconnectUsb() {
        return this._controller.Disconnect();
    }
    public Task SetupTest(List<WaferSetup> testSetup) {
        Monitor.Enter(this._testService);
        try {
            var result=this._testService.SetupTest(testSetup);
            if (result.IsSuccess) {
                this.Clients.Caller.OnTestSetupSucceeded();
            } else {
                this.Clients.Caller.OnTestSetupFailed(result.Error);
            }
        } finally {
            Monitor.Exit(this._testService);
        }
        return Task.CompletedTask;
    }

    public Task SendStartTest() {
        return this._controller.Send(ArduinoMsgPrefix.CommandPrefix, ArduinoCommand.Start);
    }
    
    public Task SendCommand(ArduinoCommand command) {
        return this._controller.Send(ArduinoMsgPrefix.CommandPrefix,command);
    }

    public Task SendId(string newId) {
        return this._controller.Send(ArduinoMsgPrefix.IdReceive,new StationIdPacket() { StationId = newId });
    }
    
    public Task RequestId() {
        return this._controller.Send(ArduinoMsgPrefix.IdRequest,ArduinoMsgPrefix.IdRequest);
    }

    public async Task CheckForUpdate() {
        await this._controller.CheckForUpdate();
    }

    public async Task UpdateFirmware() {
        await this._controller.UpdateFirmware();
    }

    public Task SendProbeConfig(ProbeControllerConfig packet) {
        return this._controller.Send(ArduinoMsgPrefix.ProbeConfigPrefix,packet);
    }
    
    public Task SendHeaterConfig(HeaterControllerConfig packet) {
        return this._controller.Send(ArduinoMsgPrefix.HeaterConfigPrefix,packet);
    }
    
    public Task SendStationConfig(StationConfiguration packet) {
        return this._controller.Send(ArduinoMsgPrefix.StationConfigPrefix,packet);
    }

    public Task SendFirmwareVersion(string newVersion) {
        return this._controller.Send(ArduinoMsgPrefix.VersionReceive,new StationVersionPacket() { Version = newVersion });
    }
}
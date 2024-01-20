using BurnIn.Shared.Controller;
using BurnIn.Shared.Models.BurnInStationData;
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

    public Task<ControllerResult> ExecuteCommand(ArduinoCommand command) {
        return this._controller.ExecuteCommand(command);
    }
    
    public Task<ControllerResult> ToggleHeater() {
        //return this._controller.ExecuteCommand(ArduinoCommand.HeaterToggle);
        return null;
    }

    public Task<ControllerResult> UpdateArduinoSettings(StationConfiguration config) {
        int enabled = config.SwitchingEnabled ? 1 : 0;
        string command = $"U{enabled}{config.DefaultCurrent.Name}{config.TemperatureSetPoint}";
        //return this._controller.ExecuteCommand(ArduinoCommand.Update,command);
        return null;
    }
}
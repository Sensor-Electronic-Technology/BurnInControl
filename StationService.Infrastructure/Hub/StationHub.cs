using BurnInControl.Shared.ComDefinitions;
using BurnInControl.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
namespace StationService.Infrastructure.Hub;

public class StationHub:Hub<IStationHub> {
    public async Task SendSerialCom(StationSerialData serialData) {
        await this.Clients.All.OnSerialCom(serialData);
    }
    
    public async Task SendSerialComMessage(string message) {
        await this.Clients.All.OnSerialComMessage(message);
    }
}
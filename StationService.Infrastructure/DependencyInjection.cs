using BurnInControl.Application.StationControl.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StationService.Infrastructure.Firmware;
using StationService.Infrastructure.Hosted;
using StationService.Infrastructure.SerialCom;
using StationService.Infrastructure.StationControl;
using StationService.Infrastructure.TestLogs;
using System.Threading.Channels;
namespace StationService.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddStationService(this IServiceCollection services) {
        var channel = Channel.CreateUnbounded<string>();
        services.AddMediatR(config => {
            
        });
        services.AddSingleton(channel.Reader);
        services.AddSingleton(channel.Writer);
        services.AddSingleton<BurnInTestService>();
        services.AddSingleton<FirmwareUpdateService>();
        services.AddSingleton<IStationController,StationController>();
        services.AddSingleton<UsbController>();
        /*services.AddSingleton<StationMessageHandler>();*/
        services.AddHostedService<StationWorkerService>();
        return services;
    }
}
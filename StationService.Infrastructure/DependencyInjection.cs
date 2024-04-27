using BurnInControl.Application.FirmwareUpdate.Handlers;
using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Application.ProcessSerial.Handlers;
using BurnInControl.Application.ProcessSerial.Interfaces;
using BurnInControl.Application.StationControl.Handlers;
using BurnInControl.Application.StationControl.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StationService.Infrastructure.Firmware;
using StationService.Infrastructure.Hosted;
using StationService.Infrastructure.SerialCom;
using StationService.Infrastructure.StationControl;
using StationService.Infrastructure.TestLogs;
using System.Threading.Channels;
using BurnInControl.Application.BurnInTest.Handlers;
using BurnInControl.Application.BurnInTest.Interfaces;
using Coravel;
using StationService.Infrastructure.Firmware.Jobs;

namespace StationService.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddStationService(this IServiceCollection services) {
        var channel = Channel.CreateUnbounded<string>();
        services.AddSingleton(channel.Reader);
        services.AddSingleton(channel.Writer);
        services.AddSingleton<ITestService,TestService>();
        services.AddSingleton<IFirmwareUpdateService,FirmwareUpdateService>();
        services.AddSingleton<IStationController,StationController>();
        services.AddSingleton<UsbController>();
        services.AddTransient<IFirmwareUpdateJob,FirmwareUpdateJob>();
        services.AddSingleton<IStationMessageHandler,StationMessageHandler>();
        services.AddHostedService<StationWorkerService>();
        services.AddHostedService<UpdateWatcher>();
        
        services.AddMediatR(config => {
            config.RegisterServicesFromAssemblies(
            typeof(StationSerialMessageHandler).Assembly,
            typeof(ConnectionActionHandler).Assembly, 
            typeof(SendStationCommandHandler).Assembly,
            typeof(StartupTryUpdateFirmwareCommandHandler).Assembly,
            typeof(TryUpdateFirmwareCommandHandler).Assembly,
            typeof(StartTestCommandHandler).Assembly,
            typeof(LogCommandHandler).Assembly,
            typeof(TestCompletedHandler).Assembly,
            typeof(TestSetupHandler).Assembly,
            typeof(SendAckHandler).Assembly,
            typeof(SendTestIdCommandHandler).Assembly,
            typeof(StartLoadStateCommandHandler).Assembly,
            typeof(StopAndSaveStateCommandHandler).Assembly,
            typeof(SendSavedStateCommandHandler).Assembly,
            typeof(HardStopCommandHandler).Assembly,
            typeof(StartFromLoadCommandHandler).Assembly,
            typeof(ConnectionStatusHandler).Assembly);
        });
        services.AddScheduler();
        return services;
    }
}
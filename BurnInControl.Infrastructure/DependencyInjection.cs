using BurnInControl.Infrastructure.ControllerTestState;
using BurnInControl.Infrastructure.FirmwareModel;
using BurnInControl.Infrastructure.QuickTest;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Infrastructure.WaferTestLogs;
using Microsoft.Extensions.DependencyInjection;
namespace BurnInControl.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddServiceDataInfrastructure(this IServiceCollection services) {
        services.AddTransient<StationDataService>();
        services.AddTransient<TestLogDataService>();
        services.AddTransient<FirmwareDataService>();
        services.AddTransient<SavedStateDataService>();
        services.AddTransient<QuickTestDataService>();
        services.AddTransient<StationConfigDataService>();
        services.AddTransient<WaferTestLogDataService>();
        return services;
    }
    
    public static IServiceCollection AddUiDataInfrastructure(this IServiceCollection services) {
        services.AddTransient<QuickTestDataService>();
        services.AddTransient<StationConfigDataService>();
        return services;
    }
}
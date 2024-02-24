using BurnInControl.Shared.AppSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace BurnInControl.Shared;

public static class DependencyInjection {
    public static IServiceCollection AddSettings(this IServiceCollection services, IHostApplicationBuilder builder) {
        services.Configure<WolverineSettings>(builder.Configuration.GetSection(nameof(WolverineSettings)));
        services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        services.Configure<FirmwareUpdateSettings>(builder.Configuration.GetSection(nameof(FirmwareUpdateSettings)));
        return services;
    }
    
    public static IServiceCollection AddUiSettings(this IServiceCollection services, IHostApplicationBuilder builder) {
        services.Configure<WolverineSettings>(builder.Configuration.GetSection(nameof(WolverineSettings)));
        services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        return services;
    }
}
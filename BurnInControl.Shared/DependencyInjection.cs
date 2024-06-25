using System.Runtime.InteropServices;
using BurnInControl.Shared.AppSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace BurnInControl.Shared;

public static class DependencyInjection {
    public static IServiceCollection AddSettings(this IServiceCollection services, IHostApplicationBuilder builder) {
        services.Configure<StationSettings>(builder.Configuration.GetSection(nameof(StationSettings)));
        services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        services.Configure<FirmwareUpdateSettings>(builder.Configuration.GetSection(nameof(FirmwareUpdateSettings)));
        services.Configure<UpdateSettings>(builder.Configuration.GetSection(nameof(UpdateSettings)));
        return services;
    }
    
    public static IServiceCollection AddUiSettings(this IServiceCollection services, IHostApplicationBuilder builder) {
        services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        return services;
    }
    
    public static IHostApplicationBuilder AddApiSettings(this IHostApplicationBuilder builder) {
        builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        return builder;
    }
    
    /*public static IServiceCollection AddApiSettings(this IServiceCollection services, IHostApplicationBuilder builder) {
        services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        return services;
    }*/
}
﻿using BurnIn.Data.AppSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace BurnIn.Data;

public static class DependencyInjection {
    public static IServiceCollection AddSettings(this IServiceCollection services, IHostApplicationBuilder builder) {
        services.Configure<WolverineSettings>(builder.Configuration.GetSection(nameof(WolverineSettings)));
        services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        return services;
    }
    
    public static IServiceCollection AddFirmwareSettings(this IServiceCollection services, IHostApplicationBuilder builder) {
        services.Configure<FirmwareUpdateSettings>(builder.Configuration.GetSection(nameof(FirmwareUpdateSettings)));
        return services;
    }
}
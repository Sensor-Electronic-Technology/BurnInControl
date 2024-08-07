﻿using BurnInControl.Infrastructure.ControllerTestState;
using BurnInControl.Infrastructure.FirmwareModel;
using BurnInControl.Infrastructure.QuickTest;
using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Infrastructure.WaferTestLogs;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
namespace BurnInControl.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) {
        services.AddPersistence();
        return services;
    }
    
    public static IServiceCollection AddPersistence(this IServiceCollection services) {
        services.AddTransient<StationDataService>();
        services.AddTransient<TestLogDataService>();
        services.AddTransient<FirmwareDataService>();
        services.AddTransient<SavedStateDataService>();
        services.AddTransient<QuickTestDataService>();
        services.AddTransient<StationConfigDataService>();
        services.AddTransient<WaferTestLogDataService>(); 
        return services;
    }
    
    public static IServiceCollection AddApiPersistence(this IServiceCollection services) {
        services.AddTransient<StationDataService>();
        services.AddTransient<TestLogDataService>();
        services.AddTransient<QuickTestDataService>();
        services.AddTransient<WaferTestLogDataService>(); 
        return services;
    }
    
    public static IServiceCollection AddDashboardPersistence(this IServiceCollection services) {
        services.AddScoped<SavedStateDataService>();
        services.AddScoped<StationConfigDataService>();
        services.AddScoped<WaferTestLogDataService>(); 
        services.AddScoped<StationDataService>();
        services.AddScoped<TestLogDataService>();
        return services;
    }
    
}
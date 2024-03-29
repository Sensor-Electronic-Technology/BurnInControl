﻿using BurnInControl.Infrastructure.StationModel;
using BurnInControl.Infrastructure.TestLogs;
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
        return services;
    }
    
}
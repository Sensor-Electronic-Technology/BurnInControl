using BurnInControl.Infrastructure.StationModel;
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
        services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:28080"));
        services.AddScoped<StationDataService>();
        services.AddScoped<TestLogDataService>();
        return services;
    }
    
}
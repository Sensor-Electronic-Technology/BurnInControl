using System.Runtime.InteropServices;
using BurnInControl.Infrastructure;
using MongoDB.Driver;
using Serilog;
using BurnInControl.Shared;
using StationService.Infrastructure;
using StationService.Infrastructure.Hub;

var builder = WebApplication.CreateBuilder(args);
string? connectionString;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
    connectionString=builder.Configuration.GetConnectionString("LocalConnection");
    connectionString ??= "mongodb://192.168.68.111:27017";
} else {
    connectionString=builder.Configuration.GetConnectionString("DefaultConnection");
    connectionString ??= "mongodb://172.20.3.41:27017";
}
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
builder.Services.AddInfrastructure();
builder.Services.AddSettings(builder);
builder.Services.AddStationService();
builder.Services.AddSignalR(options => { 
    options.EnableDetailedErrors = true;
});
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
var app = builder.Build();
/*app.Services.UseScheduler(scheduler => {
    //scheduler.Schedule<FirmwareUpdateJob>().EverySeconds(30).RunOnceAtStart();
}).LogScheduledTaskProgress(app.Services.GetRequiredService<ILogger<IScheduler>>());*/

app.MapHub<StationHub>("/hubs/station");
app.Run();
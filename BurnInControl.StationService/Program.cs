using BurnInControl.Infrastructure;
using MongoDB.Driver;
using Serilog;
using BurnInControl.Shared;
using StationService.Infrastructure;
using StationService.Infrastructure.Hub;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
builder.Services.AddInfrastructure();
builder.Services.AddSettings(builder);
builder.Services.AddStationService();
builder.Services.AddSignalR(options => { 
    options.EnableDetailedErrors = true;
});
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://mongodb:27017"));
var app = builder.Build();
app.MapHub<StationHub>("/hubs/station");
app.Run();
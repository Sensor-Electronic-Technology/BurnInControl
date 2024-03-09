using BurnInControl.Application.ProcessSerial.Messages;
using BurnInControl.Application.StationControl.Handlers;
using BurnInControl.Infrastructure;
using BurnInControl.Shared.AppSettings;
using MongoDB.Driver;
using Serilog;
using Wolverine;
using BurnInControl.Shared;
using JasperFx.Core;
using StationService.Infrastructure;
using StationService.Infrastructure.Hub;
using StationService.Infrastructure.SerialCom;
using Wolverine.Transports.Tcp;



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(opts => {
    var config = builder.Configuration.GetSection(nameof(WolverineSettings))
        .Get<WolverineSettings>();
    
    opts.ListenAtPort(config?.ListenPort ?? 5580);
    opts.LocalQueue(config?.ControllerQueue ?? "ControllerCommandQueue");
    opts.LocalQueue("StationMessageQueue");
    opts.PublishMessage<StationMessage>().ToLocalQueue("StationMessageQueue");
    opts.Discovery.IncludeAssembly(typeof(StationMessageHandler).Assembly);
    opts.Discovery.IncludeType<SendStationCommandHandler>();
    opts.Discovery.IncludeType<StationMessageHandler>();
});

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

/*builder.Services.Configure<FirmwareUpdateSettings>(builder.Configuration.GetSection(nameof(FirmwareUpdateSettings)));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));*/

builder.Services.AddInfrastructure();
builder.Services.AddSettings(builder);
builder.Services.AddStationService();
builder.Services.AddSignalR(options => { 
    options.EnableDetailedErrors = true;
}); 

builder.Services.AddLogging();
/*builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://192.168.68.112:27017"));*/
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://mongodb:27017"));
var app = builder.Build();
app.MapHub<StationHub>("/hubs/station");
//app.MapGet("/", () => "Hello World!");
app.Run();
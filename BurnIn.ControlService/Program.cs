using BurnIn.ControlService;
using BurnIn.Data.AppSettings;
using JasperFx.Core;
using Microsoft.Extensions.Hosting.Systemd;
using MongoDB.Driver;
using Serilog;
using System.Threading.Channels;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Transports.Tcp;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(opts => {
    var config = builder.Configuration.GetSection(nameof(WolverineSettings))
        .Get<WolverineSettings>();
    opts.ListenAtPort(config?.ListenPort ?? 5580);
    opts.LocalQueue(config?.ControllerQueue ?? "ControllerCommandQueue");
    //opts.Publish<StationComm>()
});

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.Configure<FirmwareUpdateSettings>(builder.Configuration.GetSection(nameof(FirmwareUpdateSettings)));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
builder.Services.AddSignalR(options => { 
    options.EnableDetailedErrors = true;
}); 
var channel = Channel.CreateUnbounded<string>();
builder.Host.UseSystemd();
builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:28080"));
builder.Services.AddTransient<BurnInTestService>();
builder.Services.AddTransient<FirmwareUpdateService>();
builder.Services.AddSingleton<StationController>();
builder.Services.AddSingleton<UsbController>();
builder.Services.AddHostedService<StationWorkerService>();
var app = builder.Build();


//app.Urls.Add("http://192.168.68.112:3000");
//app.Urls.Add("http://172.20.1.15:3000");
app.MapHub<StationHub>("/hubs/station");

//app.MapGet("/", () => "Hello World!");

app.Run();
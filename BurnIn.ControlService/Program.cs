using BurnIn.ControlService.Services;
using BurnIn.ControlService.AppSettings;
using BurnIn.ControlService.Hubs;
using BurnIn.Shared.Hubs;
using Microsoft.Extensions.Hosting.Systemd;
using MongoDB.Driver;
using System.Threading.Channels;
using StationController=BurnIn.ControlService.Services.StationController;
using UsbController=BurnIn.ControlService.Services.UsbController;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FirmwareVersionSettings>(builder.Configuration.GetSection(nameof(FirmwareVersionSettings)));
builder.Services.Configure<DatabaseConnections>(builder.Configuration.GetSection(nameof(DatabaseConnections)));
builder.Services.AddSignalR(options => 
{ 
    options.EnableDetailedErrors = true;
}); 
var channel = Channel.CreateUnbounded<string>();
builder.Host.UseSystemd();
builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:28080"));
builder.Services.AddTransient<BurnInTestService>();
builder.Services.AddTransient<FirmwareVersionService>();
builder.Services.AddSingleton<StationController>();
builder.Services.AddSingleton<UsbController>();
builder.Services.AddHostedService<StationService>();



var app = builder.Build();

//app.Urls.Add("http://192.168.68.112:3000");
app.Urls.Add("http://172.20.1.15:3000");
app.MapHub<StationHub>("/hubs/station");

//app.MapGet("/", () => "Hello World!");

app.Run();
using BurnIn.ControlService;
using BurnIn.Shared.Controller;
using BurnIn.Shared.Hubs;
using System.Threading.Channels;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
var channel = Channel.CreateUnbounded<string>();
builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);
builder.Services.AddLogging();
builder.Services.AddSingleton<StationController>();
builder.Services.AddSingleton<UsbController>();
builder.Services.AddHostedService<StationService>();



var app = builder.Build();
//app.Urls.Add("http://192.168.68.112:3000");
app.MapHub<StationHub>("/hubs/station");

//app.MapGet("/", () => "Hello World!");

app.Run();
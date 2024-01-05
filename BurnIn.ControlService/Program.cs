using BurnIn.ControlService;
using BurnIn.Shared.Controller;
using BurnIn.Shared.Hubs;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<StationController>();
builder.Services.AddSingleton<UsbController>();
builder.Services.AddHostedService<StationService>();



var app = builder.Build();
app.Urls.Add("http://192.168.68.112:3000");
app.MapHub<StationHub>("/hubs/station");

//app.MapGet("/", () => "Hello World!");

app.Run();
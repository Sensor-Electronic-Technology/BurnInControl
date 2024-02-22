using BurnIn.BlazorUI.Components;
using BurnIn.ControlService.Infrastructure.Services;
using BurnIn.Shared.AppSettings;
using BurnIn.ControlService.Infrastructure.HostedServices;
using BurnIn.ControlService.Infrastructure.Hubs;
using BurnIn.Data.AppSettings;
using DevExpress.Xpo.Logger;
using Wolverine;
using Wolverine.Transports.Tcp;
using MongoDB.Driver;
using System.Threading.Channels;
using Wolverine.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<FirmwareUpdateSettings>(builder.Configuration.GetSection(nameof(FirmwareUpdateSettings)));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
builder.Services.AddSignalR(options => { 
    options.EnableDetailedErrors = true;
}); 

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
});

var channel = Channel.CreateUnbounded<string>();
//builder.Host.UseSystemd();
builder.Host.UseWolverine(opts => {
    opts.ListenAtPort(5580);
    opts.PublishAllMessages().ToPort(5581);
    opts.OnException<InvalidOperationException>().Discard();
});

builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:28080"));
builder.Services.AddSingleton<BurnInTestService>();
builder.Services.AddSingleton<FirmwareUpdateService>();
builder.Services.AddSingleton<StationController>();
builder.Services.AddSingleton<UsbController>();

builder.Services.AddHostedService<StationService>();

var app = builder.Build();
//app.Urls.Add("http://172.20.1.15:3000");
app.MapHub<StationHub>("/hubs/station");

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
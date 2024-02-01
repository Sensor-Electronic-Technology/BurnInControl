using BurnIn.BlazorUI.Components;
using BurnIn.Shared.AppSettings;
using BurnIn.Shared.Hubs;
using BurnIn.Shared.Services;
using MongoDB.Driver;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<FirmwareVersionSettings>(builder.Configuration.GetSection(nameof(FirmwareVersionSettings)));
builder.Services.Configure<DatabaseConnections>(builder.Configuration.GetSection(nameof(DatabaseConnections)));
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
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
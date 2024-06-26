using System.Diagnostics;
using System.Runtime.InteropServices;
using BurnInControl.Data.StationModel;
using BurnInControl.Infrastructure;
using BurnInControl.Shared;
using Radzen;
using BurnInControl.UI.Components;
using BurnInControl.UI.Services;
using MongoDB.Driver;
using QuickTest.Data.AppSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents()
      .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

/*string? connectionString="";
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
    connectionString=builder.Configuration.GetConnectionString("LocalConnection");
    connectionString ??= "mongodb://192.168.68.111:27017";
} else {
    connectionString=builder.Configuration.GetConnectionString("DefaultConnection");
    connectionString ??= "mongodb://172.20.3.41:27017";
}*/
string connectionString=builder.Configuration.GetConnectionString("DefaultConnection") ?? "mongodb://172.20.3.41:27017";
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();
builder.Services.AddInfrastructure();
builder.Services.AddUiSettings(builder);
builder.Services.AddLogging();
builder.Services.AddScoped<HubClient>();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
builder.Services.AddSingleton<ConsoleWriter>();
builder.Services.AddSingleton<NotifyPlotOptions>();
var stationId=Environment.GetEnvironmentVariable("StationId") ?? "S99";
builder.Services.AddSingleton(new StationStatusService(stationId,StationState.Offline));
builder.Services.AddScoped<TestSetupService>();
builder.Services.AddSingleton<NotifyWaferIdChanged>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
using Radzen;
using BurnInControl.Dashboard.Components;
using BurnInControl.Dashboard.Data;
using BurnInControl.Dashboard.Services;
using BurnInControl.Infrastructure;
using BurnInControl.Infrastructure.FirmwareModel;
using BurnInControl.Shared.AppSettings;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents()
      .AddCircuitOptions(options => options.DetailedErrors = true)
      .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();
builder.Services.AddDashboardPersistence();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<StationErrorService>();
/*builder.Services.AddSingleton<VisitorTrackingService>();
builder.Services.AddSingleton<CircuitHandler,CircuitHandlerService>();*/
builder.Services.AddBlazorDownloadFile();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
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
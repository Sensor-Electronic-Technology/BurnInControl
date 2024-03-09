using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Infrastructure;
using BurnInControl.Shared;
using BurnInControl.Shared.AppSettings;
using Radzen;
using BurnInControl.UI.Components;
using BurnInControl.UI.Services;
using Microsoft.AspNetCore.DataProtection;
using MongoDB.Driver;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Transports.Tcp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents()
      .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
/*builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"\var\lib\dpkeys"));*/
builder.Services.AddHttpClient();
//builder.Host.UseSystemd();
builder.Host.UseWolverine(opts => {
    var config = builder.Configuration.GetSection(nameof(WolverineSettings))
        .Get<WolverineSettings>();
    opts.ListenAtPort(config?.ListenPort ?? 5581);
    opts.PublishMessage<SendStationCommand>().ToServerAndPort("station.service", config?.PublishPort ?? 5580);
    opts.OnException<InvalidOperationException>().Discard();
});

builder.Services.AddInfrastructure();
builder.Services.AddUiSettings(builder);
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://mongodb:27017"));
/*builder.Services.AddSingleton<ConsoleWriter>();*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/*app.UseHttpsRedirection();*/
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
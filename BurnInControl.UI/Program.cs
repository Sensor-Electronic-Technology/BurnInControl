using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Infrastructure;
using BurnInControl.Shared;
using BurnInControl.Shared.AppSettings;
using Radzen;
using BurnInControl.UI.Components;
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
builder.Services.AddHttpClient();
//builder.Host.UseSystemd();
builder.Host.UseWolverine(opts => {
    var config = builder.Configuration.GetSection(nameof(WolverineSettings))
        .Get<WolverineSettings>();
    opts.ListenAtPort(config?.ListenPort ?? 5581);
    opts.PublishMessage<SendStationCommand>().ToPort(config.PublishPort ?? 5580);
    opts.OnException<InvalidOperationException>().Discard();
});

builder.Services.AddInfrastructure();
builder.Services.AddUiSettings(builder);
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:28080"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
using BurnIn.BlazorUI.Components;
using BurnInControl.Application.StationControl.Handlers;
using BurnInControl.Application.StationControl.Messages;
using BurnInControl.Infrastructure;
using BurnInControl.Shared;
using BurnInControl.Shared.AppSettings;
using Wolverine;
using Wolverine.Transports.Tcp;
using MongoDB.Driver;
using Wolverine.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSystemd();
builder.Host.UseWolverine(opts => {
    var config = builder.Configuration.GetSection(nameof(WolverineSettings))
        .Get<WolverineSettings>();
    opts.ListenAtPort(config?.ListenPort ?? 5581);
    opts.PublishMessage<SendStationCommand>().ToPort(config.PublishPort ?? 5580);
    opts.OnException<InvalidOperationException>().Discard();
});
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
});

builder.Services.AddInfrastructure();
builder.Services.AddUiSettings(builder);
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:28080"));


var app = builder.Build();
//app.Urls.Add("http://172.20.1.15:3000");
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
using BurnIn.Shared.Controller;
using BurnIn.Shared.Hubs;
using BurnInApp;
using BurnInApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddSingleton<StationController>();
builder.Services.AddSingleton<UsbController>();
builder.Services.AddHostedService<StationService>();

var app = builder.Build();
app.Urls.Add("http://192.168.68.112:3000");
app.MapHub<StationHub>("/hubs/station");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

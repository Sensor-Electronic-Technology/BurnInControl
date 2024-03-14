using BurnInControl.Infrastructure;
using BurnInControl.Shared;
using Radzen;
using BurnInControl.UI.Components;
using BurnInControl.UI.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents()
      .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();
builder.Services.AddInfrastructure();
builder.Services.AddUiSettings(builder);
builder.Services.AddLogging();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://mongodb:27017"));
builder.Services.AddSingleton<ConsoleWriter>();

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
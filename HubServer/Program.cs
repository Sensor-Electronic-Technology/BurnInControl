
using HubServer;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<HubController>();
builder.Services.AddHostedService<HubService>();


var app = builder.Build();

app.MapHub<TestHub>("/hubs/testhub");
//app.MapGet("/", () => "Hello World!");

app.Run();
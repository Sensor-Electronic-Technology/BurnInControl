using BurnInControl.HostRunner;
using BurnInControl.HostRunner.Hubs;
using Docker.DotNet;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSystemd();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSignalR(options => { 
    options.EnableDetailedErrors = true;
});
builder.Services.AddSingleton<IDockerClient>(new DockerClientConfiguration(
        new Uri("unix:///var/run/docker.sock"))
    .CreateClient());
var host = builder.Build();

host.Urls.Add("http://localhost:4000");
host.MapHub<HostHub>("/hubs/host");
host.Run();
using System.Diagnostics;
using BurnInControl.HubDefinitions.Hubs;
using BurnInControl.Shared;
using Microsoft.AspNetCore.SignalR;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace BurnInControl.HostRunner.Hubs;
public class HostHub:Hub {
    private readonly ILogger<HostHub> _logger;
    private readonly IDockerClient _dockerClient;
    
    public HostHub(ILogger<HostHub> logger,IDockerClient dockerClient) {
        this._logger = logger;
        this._dockerClient = dockerClient;
    }
    
    public Task RestartService() {
        return this.RestartStationService();
    }

    public async Task RestartBrowser() {
        await this.CloseBrowser();
    }
    
    private async Task RestartStationService() {
        var containerId = await this.FindContainer();
        if (string.IsNullOrEmpty(containerId)) {
            this._logger.LogError("Could not find container");
            return;
        }
        await this._dockerClient.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters());
    }

    private async Task<string> FindContainer() {
        var containers = await this._dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        var stationServiceContainer=containers.FirstOrDefault(e=>e.Names.Contains("/station-service"));
        return stationServiceContainer?.ID ?? string.Empty;
    }
    
    private async Task CloseBrowser() {
        this._logger.LogInformation("Closing browser...");
        using Process process = new Process();
        process.StartInfo.FileName = "pkill";
        process.StartInfo.Arguments = "-f chromium-browser";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        try {
            process.Start();
            var result = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            Console.WriteLine(result);
            
        } catch(Exception e) {
            this._logger.LogError("Error while closing browser" +
                                  "\n  {ErrorMessage}", e.ToErrorMessage());
            
        }
    }
}
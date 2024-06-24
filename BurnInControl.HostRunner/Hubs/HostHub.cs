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
        await this.OpenBrowser();
    }
    
    private async Task RestartStationService() {
        var containerId = await this.FindContainer();
        if (string.IsNullOrEmpty(containerId)) {
            this._logger.LogError("Could not find container");
            return;
        }
        await this._dockerClient.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters());
        await this.RestartBrowser();
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
    
    private async Task OpenBrowser() {
        this._logger.LogInformation("Opening browser...");
        using Process process = new Process();
        /*process.StartInfo.FileName = "runuser";
        process.StartInfo.Arguments = "-l setitech -c 'chromium-browser --start-fullscreen http://localhost'";*/
        /*process.StartInfo.FileName = "gio";
        process.StartInfo.Arguments = " launch /home/setitech/Desktop/burninapp.desktop &";*/
        process.StartInfo.FileName = "/home/setitech/start-chrome.sh";
        process.StartInfo.WorkingDirectory = "/home/setitech";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.UserName = "setitech";
        try {
            process.Start();
            /*var result = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            Console.WriteLine(result);*/
        } catch(Exception e) {
            this._logger.LogError("Error while opening browser" +
                                  "\n  {ErrorMessage}", e.ToErrorMessage());
            
        }
    }
}
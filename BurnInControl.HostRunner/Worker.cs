namespace BurnInControl.HostRunner;

public class Worker : IHostedService,IDisposable {
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger) {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("host-runner started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("host-runner stopping");
        return Task.CompletedTask;
    }

    public void Dispose() {
        this._logger.LogInformation("host-runner disposed");
    }
}
using Microsoft.Extensions.Hosting;
namespace BurnIn.ControlService.Infrastructure.HostedServices;

public class BurnInTestHostedService:IHostedService,IDisposable {
    
    public Task StartAsync(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
    public Task StopAsync(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
    public void Dispose() {
        throw new NotImplementedException();
    }
}
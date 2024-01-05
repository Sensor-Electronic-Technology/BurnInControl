
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
namespace HubServer;

public class TestHub:Hub<ITestHub> {
    private readonly HubController _hubController;

    public TestHub(HubController hubController) {
        this._hubController = hubController; 
    }

    public async Task SetIncrement(int increment) {
        this._hubController.SetIncrement(increment);
        await Clients.All.OnGetIncrement(increment);
    }
    
    public ChannelReader<int> Counter(int count, int delay, CancellationToken token) {
        var channel = Channel.CreateUnbounded<int>();
        _ = this.WriteItemsAsync(channel.Writer, count, delay, token);
        return channel.Reader;
    }

    private async Task WriteItemsAsync(ChannelWriter<int> writer,
        int count,
        int delay,
        CancellationToken token) {
        Exception localException = null;
        try {
            for (int i = 0; i < count; i++) {
                await writer.WriteAsync(i, token);
                await Task.Delay(delay, token);
            }
        } catch(Exception ex) {
            localException = ex;
        } finally {
            writer.Complete(localException);
        }
        
    }

}
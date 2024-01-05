using HubServer;
using Microsoft.AspNetCore.SignalR;
namespace HubServer;

public class HubController {
    private double _counter;
    private double _increment;
    private readonly IHubContext<TestHub, ITestHub> _hubContext;
    private Timer _timer;

    public HubController(IHubContext<TestHub,ITestHub> hubContext) {
        this._hubContext = hubContext;
    }
    
    public Task Start() {
        this._counter = 0;
        this._increment = 1;
        this._timer = new Timer(
        callback: new TimerCallback(this.TimerHandler),
        state: null,
        dueTime: 1000,
        period: 1000);
        return Task.CompletedTask;
    }

    public void SetIncrement(int increment) {
        this._increment = increment;
    }

    private async void TimerHandler(object? state) {
        this._counter += this._increment;
        await this._hubContext.Clients.All.ShowMessage($"Counter: {this._counter}");
        //Console.WriteLine($"Counter: {this._counter}");
    }

    public Task Stop() {
        return Task.CompletedTask;
    }
    
    
}
using Stateless;
namespace BurnInControl.ConsoleTesting.TestStateMachine;

public enum ControllerState {
    UnDefined,
    StartUp,
    TryConnect,
    Idle,
    Running,
    Paused,
    Disconnected,
    Error
}

public enum ControllerTrigger {
    Startup,
    Connect,
    ConnectComplete,
    Start,
    Pause,
    Continue,
    ExecuteCommand,
    Disconnect
}

public record ControllerData{
    public bool IsConfigured { get; set; }
    public bool Connected { get; set; }
    public bool CanExecute { get; set; }
    
    public bool Error { get; set; }
    
    public ControllerData() {
        this.IsConfigured = false;
        this.Connected= false;
        this.Error = false;
    }
}

/*public record ControllerData{
    public bool IsConfigured { get; set; }
    public string StationId { get; set; }
    
    public string CurrentVersion { get; set; }
    public bool CurrentVersionSuccess { get; set; }
    
    public string LatestVersion { get; set; }
    public bool LatestVersionSuccess { get; set; }
    
    public bool UpdateAvailable { get; set; }
    public string FilePath { get; set; }
    public string UpdateOutput { get; set; }
    public bool UpdateSuccess { get; set; }
    
    public ControllerData() {
        this.IsConfigured = false;
        this.StationId= string.Empty;
        this.CurrentVersion= string.Empty;
        this.CurrentVersionSuccess= false;
        this.LatestVersion= string.Empty;
        this.LatestVersionSuccess= false;
        this.UpdateAvailable= false;
        this.FilePath= string.Empty;
        this.UpdateOutput= string.Empty;
        this.UpdateSuccess= false;
    }
}*/

public class ControllerStateMachine {
    private StateMachine<ControllerState, ControllerTrigger> _stateMachine;
    private bool _isConfigured = false;
    private ControllerData _data;
    private System.Timers.Timer? _timer;
    private DateTime _started;
    

    public ControllerStateMachine() {
        this._stateMachine = new StateMachine<ControllerState, ControllerTrigger>(ControllerState.UnDefined);
        
        this._stateMachine.OnTransitioned(this.OnTransition);

        this._stateMachine.Configure(ControllerState.UnDefined)
            .Permit(ControllerTrigger.Startup, ControllerState.StartUp)
            .OnExit(this.Configure);
        
        this._stateMachine.Configure(ControllerState.StartUp)
            .Permit(ControllerTrigger.Connect, ControllerState.TryConnect);

        this._stateMachine.Configure(ControllerState.TryConnect)
            .OnEntry(this.OnConnect)
            .PermitIf(ControllerTrigger.ConnectComplete, ControllerState.Idle, () => this._data.Connected);
        
        this._stateMachine.Configure(ControllerState.Idle)
            .Permit(ControllerTrigger.Start, ControllerState.Running)
            .Ignore(ControllerTrigger.Pause)
            .Ignore(ControllerTrigger.Continue)
            .Permit(ControllerTrigger.Disconnect, ControllerState.Disconnected);
        
        this._stateMachine.Configure(ControllerState.Running)
            .OnEntry(this.OnStart)
            .Permit(ControllerTrigger.Pause, ControllerState.Paused)
            .Ignore(ControllerTrigger.Start)
            .Ignore(ControllerTrigger.Connect)
            .Ignore(ControllerTrigger.Continue)
            .Permit(ControllerTrigger.Disconnect, ControllerState.Disconnected);

        this._stateMachine.Configure(ControllerState.Paused)
            .OnEntry(this.OnPause)
            .Permit(ControllerTrigger.Continue, ControllerState.Running)
            .Permit(ControllerTrigger.Disconnect, ControllerState.Disconnected)
            .Ignore(ControllerTrigger.Start)
            .Ignore(ControllerTrigger.Pause);

        this._stateMachine.Configure(ControllerState.Disconnected)
            .Permit(ControllerTrigger.Connect, ControllerState.TryConnect);

    }

    private void Configure() {
        this._data = new ControllerData();
        this._data.IsConfigured = true;
    }
    
    public bool StartUp() {
        if(this._stateMachine.CanFire(ControllerTrigger.Startup)) {
            this._stateMachine.Fire(ControllerTrigger.Startup);
            return true;
        }
        return false;
    }
    
    public bool Start() {
        if(this._stateMachine.CanFire(ControllerTrigger.Start)) {
            this._stateMachine.Fire(ControllerTrigger.Start);
            return true;
        }
        return false;
    }
    
    public bool Connect() {
        if(this._stateMachine.CanFire(ControllerTrigger.Connect)) {
            this._stateMachine.Fire(ControllerTrigger.Connect);
            return true;
        }
        return false;
    }
    
    public bool Disconnect() {
        if(this._stateMachine.CanFire(ControllerTrigger.Disconnect)) {
            this._stateMachine.Fire(ControllerTrigger.Disconnect);
            return true;
        }
        return false;
    }
    
    public bool Pause() {
        if(this._stateMachine.CanFire(ControllerTrigger.Pause)) {
            this._stateMachine.Fire(ControllerTrigger.Pause);
            return true;
        }
        return false;
    }
    
    public bool Continue() {
        if(this._stateMachine.CanFire(ControllerTrigger.Continue)) {
            this._stateMachine.Fire(ControllerTrigger.Continue);
            return true;
        }
        return false;
    }
    
    private void OnConnect() {
        Console.WriteLine("Connected");
        this._data.Connected = true;
        this._stateMachine.Fire(ControllerTrigger.ConnectComplete);
    }

    private void OnStart() {
        Console.WriteLine("Started");
        this.ConfigureTimer(true);
    }
    
    private void OnPause() {
        Console.WriteLine("Paused");
        this.ConfigureTimer(false);
    }
    
    private void OnContinue() {
        Console.WriteLine("Continued");
        this.ConfigureTimer(true);
    }
    
    private void TimeoutTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e) {
        Console.WriteLine($"Elapsed: {(DateTime.Now-this._started).TotalSeconds} seconds");
    }

    private void ConfigureTimer(bool active) {


            if (active) {
                this._timer = new System.Timers.Timer(500) { AutoReset = true, Enabled = false };
                this._timer.Elapsed += TimeoutTimerElapsed;
                this._timer.Start();
                this._started=DateTime.Now;
                Console.WriteLine($"Timer started.");
            } else {
                this._timer.Stop();
                this._timer.AutoReset = false;
                Console.WriteLine($"Timer stopped");
            }
    }
    
    
    
    private void OnTransition(StateMachine<ControllerState, ControllerTrigger>.Transition transition) {
        Console.WriteLine($"Transition: {transition.Source} -> {transition.Destination} : {transition.Trigger}");
    }
}
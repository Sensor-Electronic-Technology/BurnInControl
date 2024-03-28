using Automatonymous;
namespace BurnInControl.ConsoleTesting.TestStateMachine;

/*public class ControllerState {
    public State CurrentState { get; set; }
    public bool Connected { get; set; }

}

public class ControllerStateMachine:AutomatonymousStateMachine<ControllerState>{
    
    public State StartUp { get; private set; }
    public State TryConnect { get; private set; }
    public State Idle { get; private set; }
    public State Running { get; private set; }
    public State Paused { get; private set; }
    public State Disconnected { get; private set; }
    public State Error { get; private set; }
    
    public Event Startup { get; private set; }
    public Event Connect { get; private set; }
    public Event Start { get; private set; }
    public Event Pause { get; private set; }
    public Event Continue { get; private set; }
    
    public ControllerStateMachine() {
            Initially(
            Ignore(Start), 
            Ignore(Pause), 
            Ignore(Continue),
            When(Startup)
                .Then(x => {
                    x.Instance.Connected = true;
                    if (x.Instance.Connected) {
                        
                    } else {
                        x.Raise(Startup);
                    }
                    
                })
        );
        During(Idle,
            When(Start)
                .Then(x => {
                    x.Instance.Connected = true;
                    Console.WriteLine("Start");
                }),
            When(Pause)
                .Then(x => {
                    x.Instance.Connected = true;
                    Console.WriteLine("Pause");
                }),
            When(Continue)
                .Then(x => {
                    x.Instance.Connected = true;
                    Console.WriteLine("Continue");
                }),
            When(Connect)
                .Then(x => {
                    x.Instance.Connected = true;
                    Console.WriteLine("Connected");
                })
        );
        
    }
    
}*/
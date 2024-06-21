namespace BurnInControl.UI.Services;

public class NotifyWaferIdChanged {
    public event Action? WaferIdChanged;

    public Task Notify() {
        this.WaferIdChanged?.Invoke();
        return Task.CompletedTask;
    }
}
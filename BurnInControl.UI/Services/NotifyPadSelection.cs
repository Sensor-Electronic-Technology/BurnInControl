using BurnInControl.UI.Data;

namespace BurnInControl.UI.Services;

public class NotifyPadSelection {
    public event Action<PadSelection>? OnDataAvailable;
    public Task Set(PadSelection padSelection) {
        this.OnDataAvailable?.Invoke(padSelection);
        return Task.CompletedTask;
    }
}
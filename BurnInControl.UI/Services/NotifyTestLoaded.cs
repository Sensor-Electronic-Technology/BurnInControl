using BurnInControl.Data.BurnInTests.Wafers;

namespace BurnInControl.UI.Services;

public class NotifyTestLoaded {
    public event Action<List<WaferSetup>>? OnTestLoaded;

    public Task Set(List<WaferSetup> testSetup) {
        this.OnTestLoaded?.Invoke(testSetup);
        return Task.CompletedTask;
    }
}
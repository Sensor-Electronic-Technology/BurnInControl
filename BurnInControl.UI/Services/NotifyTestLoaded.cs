using BurnInControl.Data.BurnInTests.Wafers;

namespace BurnInControl.UI.Services;

public class NotifyTestLoaded {
    public event Action<List<PocketWaferSetup>>? OnTestLoaded;

    public Task Set(List<PocketWaferSetup> testSetup) {
        this.OnTestLoaded?.Invoke(testSetup);
        return Task.CompletedTask;
    }
}
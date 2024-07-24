namespace BurnInControl.Dashboard.Services;

public class StationErrorService {
    public event Action? OnErrorChanged;
    public event Action<string,List<string>>? OnErrorResolved;
    
    public Dictionary<string,List<string>> Errors { get; } = new();

    public StationErrorService() {
        for(int i=0; i<10; i++) {
            if ((i+1) < 10) {
                this.Errors.Add($"S0{i}", new());
            }else {
                this.Errors.Add($"S{i}", new());
            }
        }
    }
    
    public void NotifyError(string stationId, List<string> probeIds) {
        Console.WriteLine($"Station Id: {stationId}");
        if (this.Errors.TryGetValue(stationId, out var error)) {
            error.Clear();
            error.AddRange(probeIds);
            this.OnErrorChanged?.Invoke();
        }
    }
    
    public void NotifyErrorResolved(string stationId) {
        if (this.Errors.TryGetValue(stationId, out var error)) {
            var acknowledged=error.ToList();
            error.Clear();
            this.OnErrorResolved?.Invoke(stationId,acknowledged);
        }
    }
}
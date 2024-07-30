namespace BurnInControl.Dashboard.Services;

public class StationErrorService {
    public event Action? OnErrorChanged;
    public event Action<string,List<string>>? OnErrorResolved;
    
    public Dictionary<string,List<string>> Errors { get; } = new();

    public StationErrorService() {
        for(int i=1; i<=10; i++) {
            string stationId = (i<10) ? $"S0{i}":$"S{i}";
            this.Errors.Add(stationId,[]);
        }
    }
    
    public void NotifyError(string stationId, List<string> probeIds) {
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
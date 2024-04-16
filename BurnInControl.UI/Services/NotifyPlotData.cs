using Microsoft.AspNetCore.Components;

namespace BurnInControl.UI.Services;

public record NotifyData {
    public List<float> Temperatures { get; set; } = new List<float>();
    public List<bool> HeaterStates { get; set; } = new List<bool>();
}

public class NotifyNewPlotData {
    public event Action<NotifyData>? OnDataAvailable;

    public Task Set(List<float> tempData,List<bool> heaterStates) {
        NotifyData data=new NotifyData();
        data.Temperatures = tempData;
        data.HeaterStates = heaterStates;
        this.OnDataAvailable?.Invoke(data);
        return Task.CompletedTask;
    }

}
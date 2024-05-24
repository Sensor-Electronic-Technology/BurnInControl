using BurnInControl.UI.Data;
using Microsoft.AspNetCore.Components;
namespace BurnInControl.UI.Services;

public class NotifyPlotOptions {
    public event Action<PlotAxisOptions>? OnDataAvailable;
    public Task Set(PlotAxisOptions options) {
        PlotAxisOptions data = new PlotAxisOptions();
        data.XAxisMin = options.XAxisMin;
        data.XAxisMax = options.XAxisMax;
        data.XAxisStep = options.XAxisStep;
        data.YAxisMin = options.YAxisMin;
        data.YAxisMax = options.YAxisMax;
        data.YAxisStep = options.YAxisStep;
        this.OnDataAvailable?.Invoke(data);
        return Task.CompletedTask;
    }
}
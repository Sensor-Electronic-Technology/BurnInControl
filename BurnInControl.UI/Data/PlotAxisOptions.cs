namespace BurnInControl.UI.Data;

public class PlotAxisOptions {
    public int YAxisMin { get; set; } = 15;
    public int YAxisMax { get; set; }= 95;
    public int YAxisStep { get; set; } = 10;
    public int XAxisMin { get; set; } = 0;
    public int XAxisMax { get; set; } = 300;
    public int XAxisStep { get; set; } = 10;
    
    public PlotAxisOptions() {}
    public PlotAxisOptions(PlotAxisOptions options) {
        this.XAxisMin = options.XAxisMin;
        this.XAxisMax = options.XAxisMax;
        this.XAxisStep = options.XAxisStep;
        this.YAxisMin = options.YAxisMin;
        this.YAxisMax = options.YAxisMax;
        this.YAxisStep = options.YAxisStep;
    }
    public PlotAxisOptions Clone() {
        return (PlotAxisOptions)this.MemberwiseClone();
    }
}
namespace BurnInControl.UI.Data;

public class SvgCircle {
    public int X { get; set; }
    public int Y { get; set; }
    public int Radius { get; set; }
    public string AltTex { get; set; }
    public string PostBackValue { get; set; }
    public bool State { get; set; }
    public string Fill { get; set; } = "grey";
    public string Opacity { get; set; } = "0.5";
}
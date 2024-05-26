using Radzen;
namespace BurnInControl.UI.Data;

public class SetupAlert {
    public string Pocket { get; set; }
    public AlertStyle Style { get; set; }
    public string? Message { get; set; }
    public bool Okay { get; set; } = false;
    
    public SetupAlert? SecondaryAlert { get; set; }
}
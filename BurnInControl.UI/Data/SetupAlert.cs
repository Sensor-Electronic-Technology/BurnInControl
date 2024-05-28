using Radzen;
namespace BurnInControl.UI.Data;

public class SetupAlert {
    public string Pocket { get; set; } 
    public bool Okay { get; set; }
    public ItemAlert WaferIdAlert { get; set; }
    public ItemAlert ProbePadAlert { get; set; }

    public SetupAlert() {
        this.Pocket = "Not Set";
        this.Okay = false;
        this.WaferIdAlert = new ItemAlert();
        this.ProbePadAlert = new ItemAlert();
    }
    
}

public class ItemAlert {
    public AlertStyle Style { get; set; }
    public string? Message { get; set; }
    public bool Okay { get; set; } = false;

    public ItemAlert() {
        this.Style = AlertStyle.Success;
        this.Message = "Not Verified";
        this.Okay = false;
    }
}
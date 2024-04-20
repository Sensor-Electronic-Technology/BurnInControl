namespace BurnInControl.Shared.FirmwareData;

public class UpdateStatus {
    public string? ArduonCliOutput { get; set; }
    public string? Version { get; set; }
    public bool IsError { get; set; } = false;
    public string? ErrorMessage { get; set; } = string.Empty;
    
    public void SetUpdateStatus(string version, string avrOutput) {
        this.Version = version;
        this.ArduonCliOutput = avrOutput;
        this.IsError = false;
        this.ErrorMessage = string.Empty;
    }
    
    public void SetError(string message,string? version=default,string? avrOutput=default) {
        this.IsError = true;
        this.ErrorMessage = message;
    }
    
    public UpdateStatus() {
        //for json serialization
    }
}
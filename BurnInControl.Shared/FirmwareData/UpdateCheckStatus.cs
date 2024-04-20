namespace BurnInControl.Shared.FirmwareData;

public class UpdateCheckStatus {
    public bool UpdateAvailable { get; set; } = false;
    public string? AvailableVersion { get; set; } = string.Empty;
    public string? CurrentVersion { get; set; } = string.Empty;
    public bool IsError { get; set; } = false;
    public string? ErrorMessage { get; set; } = string.Empty;

    public void SetUpdated() {
        this.CurrentVersion=this.AvailableVersion;
        this.UpdateAvailable = false;
    }

    public void SetUpdateAvailable(string latest, string current) {
        this.UpdateAvailable = current!=latest;
        this.AvailableVersion = latest;
        this.CurrentVersion = current;
        this.IsError = false;
        this.ErrorMessage = string.Empty;
    }

    public void SetUpdateAvailableWithMessage(string message, string latest, string? current = default) {
        this.IsError = true;
        this.UpdateAvailable = true;
        this.AvailableVersion = latest;
        this.CurrentVersion=current;
        this.ErrorMessage = message;
    }

    public void SetError(string message) {
        this.UpdateAvailable = false;
        this.IsError = true;
        this.ErrorMessage = message;
    }

    public UpdateCheckStatus() {
        //for json deserialization
    }
}
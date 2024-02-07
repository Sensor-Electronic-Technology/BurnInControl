namespace BurnIn.Shared.Models;

public enum UpdateType {
    Major,
    Minor,
    Patch,
    None
}
public class FirmwareUpdateStatus {
    public bool UpdateReady { get; set; }
    public UpdateType Type { get; set; }
    public string Message { get; set; }
    public FirmwareUpdateStatus() {
        this.UpdateReady = false;
        this.Type = UpdateType.None;
        this.Message = "";
    }
    public FirmwareUpdateStatus(bool ready, UpdateType type, string message) {
        this.UpdateReady = ready;
        this.Type = type;
        this.Message = message;
    }

    public void SetNone(string? msg=null) {
        this.UpdateReady = false;
        this.Type = UpdateType.None;
        this.Message = !string.IsNullOrEmpty(msg) ? msg : "";
    }

    public void Set(UpdateType type, string msg) {
        this.UpdateReady = true;
        this.Type = type;
        this.Message = msg;
    }
}

public record FirmwareUpdateResponse {
    public string Version { get; set; }
    public string ConsoleText { get; set; }
    
    public FirmwareUpdateResponse(string version, string consoleText) {
        this.Version = version;
        this.ConsoleText = consoleText;
    }
}

public record UpdateResponseV2 {
    public bool UpdateSuccess { get; set; }
    public bool ConnectSuccess { get; set; }
    public FirmwareUpdateResponse FirmwareResponse { get; set; }

    public UpdateResponseV2() {
        this.UpdateSuccess = false;
        this.ConnectSuccess = false;
        this.FirmwareResponse = new FirmwareUpdateResponse("V.0.0","N/A");
    }
}
namespace BurnInControl.Shared.FirmwareData;

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
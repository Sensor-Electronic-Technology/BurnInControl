namespace BurnIn.Data.FirmwareData;

public record UpdateResponse {
    public string Version { get; set; }
    public string ConsoleText { get; set; }
    
    public UpdateResponse(string version, string consoleText) {
        this.Version = version;
        this.ConsoleText = consoleText;
    }
}
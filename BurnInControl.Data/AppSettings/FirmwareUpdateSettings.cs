namespace BurnIn.Data.AppSettings;

public class FirmwareUpdateSettings {
    public string GithubOrg { get; set; }
    public string GithubRepo { get; set; }
    public string FirmwarePath { get; set; }
    public string FirmwareFileName { get; set; }
    public string AvrDudeCmd { get; set; }
    public string AvrDudeFileName { get; set; }
}
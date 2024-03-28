namespace BurnInControl.Application.FirmwareUpdate.Workflow;

public class UpdateWorkflowData {
    public string StationId { get; set; }
    public string CurrentVersion { get; set; }
    public bool CurrentVersionSuccess { get; set; }
    public string LatestVersion { get; set; }
    public bool LatestVersionSuccess { get; set; }
    
    public bool UpdateAvailable { get; set; }
    
    public string FilePath { get; set; }
    
    public string UpdateOutput { get; set; }
    public bool UpdateSuccess { get; set; }
}
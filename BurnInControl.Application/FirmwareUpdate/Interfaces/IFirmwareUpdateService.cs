using BurnInControl.Shared.FirmwareData;
using ErrorOr;
namespace BurnInControl.Application.FirmwareUpdate.Interfaces;
public interface IFirmwareUpdateService {
    public bool UpdateAvailable { get; }
    public Task GetLatestVersion();
    public Task<UpdateCheckStatus> CheckForUpdate();
    public Task UploadFirmwareUpdate();
}
using ErrorOr;
namespace BurnInControl.Application.FirmwareUpdate.Interfaces;

public interface IFirmwareUpdateService {
    public Task<ErrorOr<string>> GetLatestVersion();
    public Task<ErrorOr<(string ver, string avrOutput)>> UploadFirmwareUpdate();
    public Task<ErrorOr<Success>> DownloadFirmwareUpdate();
}
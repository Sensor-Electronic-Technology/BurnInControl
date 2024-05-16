using System.Net.Http.Json;
using BurnInControl.Shared;
using BurnInControl.Shared.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickTest.Data.Constants;
using QuickTest.Data.Contracts.Responses.Get;
using ErrorOr;
using QuickTest.Data.Contracts.Requests.Get;
using QuickTest.Data.DataTransfer;

namespace BurnInControl.Infrastructure.QuickTest;

public class QuickTestDataService {
    private readonly HttpClient _client;
    private readonly ILogger<QuickTestDataService> _logger;

    public QuickTestDataService(ILogger<QuickTestDataService> logger, IOptions<DatabaseSettings> options) {
        this._logger = logger;
        this._client = new HttpClient();
        this._client.BaseAddress = new Uri(options.Value.QuickTestEndpoint ?? "http://172.20.4.206");
    }

    public async Task<ErrorOr<bool>> QuickTestExists(string waferId) {
        try {
            var result = await this._client.GetFromJsonAsync<CheckQuickTestResponse>(
                $"{QtApiPaths.CheckQuickTestPath}{waferId}");
            if (result == null) {
                this._logger.LogError("CheckQuickTest request failed, returned null");
                return Error.Failure(description: "QuickTest Check Failed.  Please check network connection");
            }
            return result.Exists;
        } catch(Exception ex) {
            string msg = ex.ToErrorMessage();
            this._logger.LogError("QuickTestExists failed, ErrorMessage: {Message}",msg);
            return false;
        }
    }

    public async Task<IEnumerable<Pad>> GetAvailablePads(string waferId) {
        try {
            var result = await this._client.GetFromJsonAsync<GetAvailableBurnInPadsResponse>(
                $"{QtApiPaths.GetAvailableBurnInPadsPath}{waferId}");
            if (result == null) {
                this._logger.LogError("GetAvailableBurnInPads request failed, returned null");
                return Enumerable.Empty<Pad>();
            }
            return result.Pads;
        } catch(Exception ex) {
            string msg = ex.ToErrorMessage();
            this._logger.LogError("GetAvailablePads failed, ErrorMessage: {Message}",msg);
            return Enumerable.Empty<Pad>();;
        }
    }

    public async Task<IEnumerable<Pad>> GetWaferMap(int waferSize) {
        try {
            var result = await this._client.GetFromJsonAsync<GetMapResponse>($"{QtApiPaths.GetMapPath}{waferSize}");
            if (result == null) {
                this._logger.LogError("GetWaferMap request failed, returned null");
                return Enumerable.Empty<Pad>();
            }
            return result.Pads;
        } catch(Exception ex) {
            string msg = ex.ToErrorMessage();
            this._logger.LogError("GetWaferMap failed, ErrorMessage: {Message}",msg);
            return Enumerable.Empty<Pad>();;
        }
    }
}
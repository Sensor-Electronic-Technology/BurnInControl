using System.Globalization;
using System.Net.Http.Json;
using BurnInControl.Shared;
using BurnInControl.Shared.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickTest.Data.Constants;
using QuickTest.Data.Contracts.Responses.Get;
using ErrorOr;
using QuickTest.Data.Contracts.Requests;
using QuickTest.Data.Contracts.Requests.Get;
using QuickTest.Data.Contracts.Responses;
using QuickTest.Data.DataTransfer;
using QuickTest.Data.Models.Wafers;

namespace BurnInControl.Infrastructure.QuickTest;

public class QuickTestDataService {
    private readonly HttpClient _client;
    private readonly ILogger<QuickTestDataService> _logger;

    public QuickTestDataService(ILogger<QuickTestDataService> logger, IOptions<DatabaseSettings> options) {
        this._logger = logger;
        this._client = new HttpClient();
        this._client.BaseAddress = new Uri(options.Value.QuickTestEndpoint ?? "http://172.20.4.206");
        //this._client.BaseAddress = new Uri(options.Value.QuickTestEndpoint ?? "http://10.5.0.13");
    }
    
    public QuickTestDataService() {
        /*this._logger = logger;*/
        this._client = new HttpClient();
        this._client.BaseAddress = new Uri("http://172.20.4.206");
    }

    public async Task<ErrorOr<bool>> QuickTestExists(string waferId) {
        try {
            var result = await this._client.GetFromJsonAsync<QtWaferExistsResponse>(
                $"{QtApiPaths.GetQuickTestExistsPath}{waferId}");
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
                return [];
            }
            return result.Pads;
        } catch(Exception ex) {
            string msg = ex.ToErrorMessage();
            this._logger.LogError("GetAvailablePads failed, ErrorMessage: {Message}",msg);
            return [];;
        }
    }

    public async Task<WaferMapDto?> GetWaferMap(int waferSize) {
        try {
            var result = await this._client.GetFromJsonAsync<GetMapResponse>($"{QtApiPaths.GetMapPath}{waferSize}");
            if (result == null) {
                this._logger.LogError("GetWaferMap request failed, returned null");
                return null;
            }
            return result.WaferMap;
        } catch(Exception ex) {
            string msg = ex.ToErrorMessage();
            this._logger.LogError("GetWaferMap failed, ErrorMessage: {Message}",msg);
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetQuickTestList(DateTime startDate) {
        try {
            var result=await this._client.GetFromJsonAsync<GetQuickTestListResponse>($"{QtApiPaths.GetQuickTestListSincePath}{startDate.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)}");
            if (result == null) {
                this._logger.LogError("GetQuickTestList request failed, returned null");
                return Enumerable.Empty<string>();
            }
            return result.WaferList;
        } catch(Exception ex) {
            string msg = ex.ToErrorMessage();
            this._logger.LogError("GetQuickTestList failed, ErrorMessage: {Message}",msg);
            return Enumerable.Empty<string>();
        }
        
    }
}
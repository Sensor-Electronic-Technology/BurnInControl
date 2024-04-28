using QuickTest.Data.Dtos;
using Microsoft.Extensions.Logging;
using QuickTest.Data.Contracts.Responses;
using QuickTest.Data.Contracts.Requests;
using QuickTest.Data.Wafer.Enums;
using System.Net.Http;
using System.Net.Http.Json;
using QuickTest.Data.Wafer;


namespace BurnInControl.Infrastructure.WaferModel;

public class WaferDataService {
    private readonly HttpClient _client;
    private readonly Logger<WaferDataService> _logger;
    
    public WaferDataService(HttpClient client, Logger<WaferDataService> logger) {
        this._client = client;
        this._logger = logger;
    }
    
    public Task<bool> Exists(string identifier) {
        throw new NotImplementedException();
    }

    /*public Task<WaferPadDto> GetWaferPads(WaferSize waferSize) {
        //this._client.GetFromJsonAsync("/api/wafer_pads/get", waferSize);
    }*/
}
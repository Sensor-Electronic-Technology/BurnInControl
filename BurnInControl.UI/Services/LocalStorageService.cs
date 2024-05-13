using System.Text.Json;
using QuickTest.Data.Contracts.Responses;

namespace BurnInControl.UI.Services;

public class LocalStorageService {
    private readonly HttpClient _quickTestClient;
    
    public LocalStorageService(HttpClient quickTestClient) {
        this._quickTestClient = quickTestClient;
    }
    
    public async Task<List<WaferPadDto>?> GetAvailableBurnInPads(int size) {
        
        /*await this._client.GetFromJsonAsync<GetMapResponse>($"api/wafer_pads/{}");
        if (response.IsSuccessStatusCode) {
            var content = await response.Content.ReadAsStringAsync();
            var pads = JsonSerializer.Deserialize<List<WaferPadDto>>(content);
            return pads;
        }*/
        return null;
    }
}
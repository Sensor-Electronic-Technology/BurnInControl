using BurnInControl.Data.BurnInTests.Wafers;
using BurnInControl.Data.StationModel;

namespace BurnInControl.UI.Services;

public class StationStatusService {
    public event Action<StationState>? OnStationStateChanged;
    public event Action<string>? OnStationIdChanged;
    public event Action<List<WaferSetup>>? OnTestSetupLoaded;

    private StationState _stationState = StationState.Offline;
    private string _stationId = "S00";
    private List<WaferSetup> _testSetup = [];

    public StationStatusService(string stationId, StationState state) {
        this._stationId = stationId;
        this._stationState = state;
    }

    public StationState StationState {
        get => this._stationState;
        set {
            this._stationState = value;
            this.OnStationStateChanged?.Invoke(value);
        }
    }

    public string StationId {
        get => this._stationId;
        set {
            this._stationId = value;
            this.OnStationIdChanged?.Invoke(value);
        }
    }

    public List<WaferSetup> TestSetup {
        get => this._testSetup;
        set {
            this._testSetup = value;
            this.OnTestSetupLoaded?.Invoke(value);
        }
    }
}
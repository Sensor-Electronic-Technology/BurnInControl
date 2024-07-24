using BurnInControl.Data.BurnInTests;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Infrastructure.WaferTestLogs;

namespace BurnInControl.Infrastructure.Dashboard;

public class DashboardTestService {
    private readonly TestLogDataService _testLogDataService;
    private readonly WaferTestLogDataService _waferLogDataService;
    
    public DashboardTestService(TestLogDataService testLogDataService, WaferTestLogDataService waferLogDataService) {
        _testLogDataService = testLogDataService;
        _waferLogDataService = waferLogDataService;
    }

    public async Task<List<List<WaferTestReading>>> GetWaferReadings(string waferId) {
        var waferTestLog = await _waferLogDataService.GetWaferTestLog(waferId);
        if (waferTestLog == null) {
            return [];
        }
        var readings = new List<List<WaferTestReading>>();
        foreach(var test in waferTestLog.WaferTests) {
            var testLog = await _testLogDataService.GetTestLogReadings(test.TestId, test.Pocket);
            readings.Add(testLog);
        }

        return readings;
    }
    
    public async Task<WaferTestLog?> GetWaferTestLog(string waferId) {
        return await _waferLogDataService.GetWaferTestLog(waferId);
    }
}
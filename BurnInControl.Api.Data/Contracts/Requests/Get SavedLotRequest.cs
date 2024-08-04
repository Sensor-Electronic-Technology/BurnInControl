using BurnInControl.Data.BurnInTests;
using BurnInControl.Shared.ComDefinitions;

namespace BurnInControl.Api.Data.Contracts.Requests;

public class GetSavedLotRequest {
    public string TestId { get; set; }
}

public class GetNotCompletedTestsRequest{
   public string StationId { get; set; }
}

public class GetTestLogRequest {
    public string TestId { get; set; }
}

public class StartNewTestRequest {
    public BurnInTestLog TestLog { get; set; }
}

public class DeleteTestLogRequest {
    public string TestId { get; set; }
}

public class UpdateAndStartTestRequest {
    public string? TestId { get; set; }
    public string? StationId { get; set; }
    public DateTime StartTime { get; set; }
    public StationSerialData? Data { get; set; }
}

public class InsertTestReadingRequest {
    public string? TestId { get; set; }
    public StationSerialData? Data { get; set; }
}

public class MarkTestCompletedRequest {
    public string? TestId { get; set; }
    public string? StationId { get; set; }
    public DateTime StopTime { get; set; }
    public ulong RunTime { get; set; }
}

public class GetLastTestLogRequest {
    public string? StationId { get; set; }
}

public class GetTestLogReadingsRequest {
    public string? TestId { get; set; }
    public string? StationPocket { get; set; }
}

public class GetStationTestReadingsRequest {
    public string? TestId { get; set; }
    public int N { get; set; }
}

public class GetWaferTestReadingsRequest {
    public string? TestId { get; set; }
    public int N { get; set; }
}

public class GetRecentStationTestsRequest {
    public string? StationId { get; set; }
    public int Limit { get; set; }
}

public class GetWaferTestsRequest {
    public string? WaferId { get; set; }
}

public class GetRecentWaferListRequest {
    public int Days { get; set; }
}

public class GetWaferTestResultsRequest {
    public string? TestId { get; set; }
}




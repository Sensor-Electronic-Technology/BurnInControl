using BurnInControl.Api.Data.Contracts.Requests;
using BurnInControl.Api.Data.Contracts.Responses;
using BurnInControl.Infrastructure.TestLogs;
using BurnInControl.Infrastructure.WaferTestLogs;
using FastEndpoints;

namespace BurnInControl.Api.Endpoints;

public class GetBurnInResultEndpointV2:Endpoint<GetBurnInResultRequest,GetBurnInResultResponse> {
    private TestLogDataService _testLogDataService;
    
    public GetBurnInResultEndpointV2(TestLogDataService testLogDataService) {
        this._testLogDataService = testLogDataService;
    }
    
    public override void Configure() {
        Get("/api/burn-in/results/v2/{waferId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetBurnInResultRequest req, CancellationToken ct) {
        if(req.WaferId is null) {
            ThrowError("Wafer Id is null");
        }
        ThrowIfAnyErrors();
        var result = await this._testLogDataService.GetExcelBurnInResult(req.WaferId);
        await SendAsync(new GetBurnInResultResponse() { Row = result }, cancellation: ct);
    }
}
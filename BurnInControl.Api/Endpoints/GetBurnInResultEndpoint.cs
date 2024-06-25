using BurnInControl.Api.Data.Contracts.Requests;
using BurnInControl.Api.Data.Contracts.Responses;
using BurnInControl.Infrastructure.WaferTestLogs;
using FastEndpoints;

namespace BurnInControl.Api.Endpoints;

public class GetBurnInResultEndpoint:Endpoint<GetBurnInResultRequest,GetBurnInResultResponse> {
    
    private WaferTestLogDataService _waferLogDataService;
    
    public GetBurnInResultEndpoint(WaferTestLogDataService waferLogDataService) {
        this._waferLogDataService = waferLogDataService;
    }
    
    public override void Configure() {
        Get("/api/burn-in/results/{waferId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetBurnInResultRequest req, CancellationToken ct) {
        if(req.WaferId is null) {
            ThrowError("Wafer Id is null");
        }
        ThrowIfAnyErrors();
        var result=await this._waferLogDataService.GetWaferBurnInResult(req.WaferId);
        await SendAsync(new GetBurnInResultResponse() { Row = result }, cancellation: ct);
    }
}
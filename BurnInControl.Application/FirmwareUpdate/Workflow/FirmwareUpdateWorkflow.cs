using BurnInControl.Application.FirmwareUpdate.Interfaces;
using BurnInControl.Infrastructure.StationModel;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using WorkflowCore.Models;
namespace BurnInControl.Application.FirmwareUpdate.Workflow;

public class FirmwareUpdateWorkflow:IWorkflow<UpdateWorkflowData> {
    private readonly ILogger<FirmwareUpdateWorkflow> _logger;
    public String Id { get; }
    public Int32 Version { get; }
    
    public FirmwareUpdateWorkflow(ILogger<FirmwareUpdateWorkflow> logger) {
        this.Id= nameof(FirmwareUpdateWorkflow);
        this.Version=1;
        this._logger=logger;
    }
    
    public void Build(IWorkflowBuilder<UpdateWorkflowData> builder) {
        var getLatest = builder.StartWith<CheckForUpdate>()
            .Output(state => state.LatestVersion, step => step.LatestVersion)
            .Output(state=>state.LatestVersionSuccess, step=>step.Success);
        var getCurrent = builder.StartWith<GetCurrent>()
            .Input(step=>step.StationId, state=>state.StationId)
            .Output(state=>state.CurrentVersion, step=>step.CurrentVersion)
            .Output(state=>state.CurrentVersionSuccess, step=>step.Success);

        builder.StartWith(context => {
            this._logger.LogInformation("Starting FirmwareUpdateWorkflow");
            return ExecutionResult.Next();
        }).Saga(saga =>
            saga.Then(getLatest)
                .CancelCondition(data => data.LatestVersionSuccess == false)
                .Then(getCurrent)
                .CancelCondition(data => data.CurrentVersionSuccess == false)
        ).OnError(WorkflowErrorHandling.Retry, TimeSpan.FromMinutes(5));



    }
}

public class CheckForUpdate: IStepBody {
    private readonly IFirmwareUpdateService _updateService;
    public string LatestVersion { get; set; } = string.Empty;
    public bool Success { get; set; } = false;
    
    public CheckForUpdate(IFirmwareUpdateService updateService) {
        this._updateService= updateService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context) {
        var result=await this._updateService.GetLatestVersion();
        if (!result.IsError) {
            this.LatestVersion=result.Value;
            this.Success=true;
        }

        return ExecutionResult.Next();
    }
}

public class GetCurrent: IStepBody {
    private readonly StationDataService _dataService;
    public string StationId { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public bool Success { get; set; } = false;
    
    public GetCurrent(StationDataService dataService) {
        this._dataService= dataService;
    }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context) {
        var result = await this._dataService.GetFirmwareVersion(this.StationId);
        if (!result.IsError) {
            this.CurrentVersion=result.Value;
            this.Success=true;
        }
        return ExecutionResult.Next();
    }
}
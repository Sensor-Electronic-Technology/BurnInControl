using WorkflowCore.Interface;
using WorkflowCore.Models;
namespace BurnInControl.ConsoleTesting.TestWorkflow;

public class DataWorkflow:IWorkflow<WorkflowData> {
    public String Id { get; private set; }
    public Int32 Version { get; }
    public void Build(IWorkflowBuilder<WorkflowData> builder) {
        Id = nameof(DataWorkflow);
        builder.StartWith(context => {
                Console.WriteLine("Starting Workflow");
                return ExecutionResult.Next();
            }).Then<Step1>()
            .Output(data => data.Success, step => step.Success)
            .Output(data => data.Message, step => step.Message)
            .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(5));
    }

}

public class Step1: IStepBody {
    public bool Success { get; set; }
    public string Message { get; set; }
    public Task<ExecutionResult> RunAsync(IStepExecutionContext context) {
        this.Success=true;
        this.Message="Step 1 Success";
        throw new Exception("Exception");
        return Task.FromResult(ExecutionResult.Next());
    }
}

using Temporalio.Workflows;

namespace NexusPOC.Payments.Nexus
{
    [Workflow]
    public class CreateOrderProxyWorkflow
    {
        [WorkflowRun]
        public async Task<CreateOrderResponse> RunAsync(CreateOrderRequest request)
        {
            return await Workflow.ExecuteActivityAsync(
                (ProxyActivities a) => a.CreateOrderUpdate(request),
                new ActivityOptions { RetryPolicy = new() { MaximumAttempts = 1 }, StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        }
    }
}

using Temporalio.Workflows;

namespace NexusPOC.Payments.Nexus
{
    [Workflow]
    public class SetFulfillmentStatusUpdateWorkflow
    {
        [WorkflowRun]
        public Task<bool> RunAsync(SetFulfillmentStatus request)
        {
            return Workflow.ExecuteActivityAsync(
                (UpdateProcessPaymentActivities a) => a.SetFulfillmentStatus(request),
                new()
                {
                    RetryPolicy = new() { MaximumAttempts = 1 },
                    StartToCloseTimeout = request.StartToCloseTimeout,
                });
        }
    }
}

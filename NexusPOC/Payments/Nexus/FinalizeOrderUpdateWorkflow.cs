using Temporalio.Workflows;

namespace NexusPOC.Payments.Nexus
{
    [Workflow]
    public class FinalizeOrderUpdateWorkflow
    {
        [WorkflowRun]
        public Task<PaymentDecision?> RunAsync(FinalizeOrderRequest request)
        {
            return Workflow.ExecuteActivityAsync(
                (UpdateProcessPaymentActivities a) => a.FinalizeOrder(request),
                new()
                {
                    RetryPolicy = new() { MaximumAttempts = 1 },
                    StartToCloseTimeout = request.StartToCloseTimeout,
                });
        }
    }
}

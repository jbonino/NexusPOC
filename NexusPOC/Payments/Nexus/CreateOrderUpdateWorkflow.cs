using Temporalio.Workflows;

namespace NexusPOC.Payments.Nexus
{
    [Workflow]
    public class CreateOrderUpdateWorkflow
    {
        [WorkflowRun]
        public Task<PaymentDecision?> RunAsync(CreateOrderRequest request)
        {
            return Workflow.ExecuteActivityAsync(
                (UpdateProcessPaymentActivities a) => a.CreateOrder(request),
                new()
                {
                    RetryPolicy = new() { MaximumAttempts = 1 },
                    StartToCloseTimeout = request.StartToCloseTimeout,
                });
        }
    }
}

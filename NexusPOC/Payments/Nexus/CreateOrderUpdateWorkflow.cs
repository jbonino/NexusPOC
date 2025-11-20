using Temporalio.Workflows;

namespace NexusPOC.Payments.Nexus
{
    [Workflow]
    public class CreateOrderUpdateWorkflow
    {
        [WorkflowRun]
        public Task<PaymentDecision?> RunAsync(CreateOrderRequest? createOrderRequest)
        {
            if (createOrderRequest is not null)
            {
                return Workflow.ExecuteActivityAsync(
                    (UpdateProcessPaymentActivities a) => a.CreateOrderUpdate(createOrderRequest),
                    new ActivityOptions
                    {
                        RetryPolicy = new() { MaximumAttempts = 1 },
                        StartToCloseTimeout = TimeSpan.FromMinutes(5),
                        CancellationToken = Workflow.CancellationToken,
                    });
            }

            return Task.FromResult((PaymentDecision?)null);
        }
    }
}

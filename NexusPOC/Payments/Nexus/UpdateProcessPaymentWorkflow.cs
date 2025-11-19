using Temporalio.Workflows;

namespace NexusPOC.Payments.Nexus
{
    [Workflow]
    public class UpdateProcessPaymentWorkflow
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
                        StartToCloseTimeout = TimeSpan.FromMinutes(5)
                    });
            }

            return Task.FromResult((PaymentDecision?)null);
        }
    }
}

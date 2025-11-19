using Temporalio.Activities;
using Temporalio.Client;

namespace NexusPOC.Payments.Nexus
{
    public class UpdateProcessPaymentActivities
    {
        private readonly ITemporalClient _client;

        public UpdateProcessPaymentActivities(ITemporalClient client)
        {
            _client = client;
        }

        [Activity]
        public async Task<PaymentDecision?> CreateOrderUpdate(CreateOrderRequest request)
        {
            var handle = _client.GetWorkflowHandle<ProcessPaymentWorkflow>(request.MeijerOrderId);
            return await handle.ExecuteUpdateAsync(wf => wf.CreateOrder(request));
        }
    }
}

using Temporalio.Activities;
using Temporalio.Client;

namespace NexusPOC.Payments.Nexus
{
    public class ProxyActivities
    {
        private readonly ITemporalClient _client;

        public ProxyActivities(ITemporalClient client)
        {
            _client = client;
        }

        [Activity]
        public async Task<CreateOrderResponse> CreateOrderUpdate(CreateOrderRequest request)
        {
            var handle = _client.GetWorkflowHandle<ProcessPaymentWorkflow>(request.MeijerOrderId);
            return await handle.ExecuteUpdateAsync(wf => wf.CreateOrder(request));
        }
    }
}

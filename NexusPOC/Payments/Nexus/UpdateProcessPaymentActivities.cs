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
        public Task<PaymentDecision?> CreateOrder(CreateOrderRequest request)
        {
            var handle = _client.GetWorkflowHandle<ProcessPaymentWorkflow>(request.MeijerOrderId);
            return handle.ExecuteUpdateAsync(wf => wf.CreateOrder(request));
        }

        [Activity]
        public Task<PaymentDecision?> FinalizeOrder(FinalizeOrderRequest request)
        {
            var handle = _client.GetWorkflowHandle<ProcessPaymentWorkflow>(request.MeijerOrderId);
            return handle.ExecuteUpdateAsync(wf => wf.FinalizeOrder(request));
        }

        [Activity]
        public Task<bool> SetFulfillmentStatus(SetFulfillmentStatus request)
        {
            var handle = _client.GetWorkflowHandle<ProcessPaymentWorkflow>(request.MeijerOrderId);
            return handle.ExecuteUpdateAsync(wf => wf.SetFulfillmentStatus(request));
        }
    }
}

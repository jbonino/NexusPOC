using NexusPOC.Payments;
using NexusPOC.Payments.Nexus;
using Temporalio.Workflows;

namespace NexusPOC.Orders
{
    [Workflow]
    public class ProcessOrderWorkflow
    {
        [WorkflowRun]
        public async Task<CreateOrderResponse> RunAsync(string meijerOrderId)
        {
            var paymentService = Workflow.CreateNexusClient<IPaymentsService>(IPaymentsService.EndpointName);
            var authAmount = 100m;

            var createOrderResponse = await paymentService
                .ExecuteNexusOperationAsync(s => s.CreateOrder(new CreateOrderRequest(meijerOrderId, authAmount)));

            return createOrderResponse;
        }
    }
}

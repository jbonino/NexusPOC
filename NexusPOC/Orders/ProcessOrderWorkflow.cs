using NexusPOC.Payments;
using NexusPOC.Payments.Nexus;
using Temporalio.Workflows;

namespace NexusPOC.Orders
{
    [Workflow]
    public class ProcessOrderWorkflow
    {
        [WorkflowRun]
        public async Task<PaymentDecision> RunAsync(string meijerOrderId)
        {
            Console.WriteLine("ProcessOrderWorkflow.RunAsync...");
            var paymentService = Workflow.CreateNexusClient<IPaymentsService>(IPaymentsService.EndpointName);

            var timeout = TimeSpan.FromSeconds(60);
            var createOrderPaymentRequest = new CreateOrderRequest(meijerOrderId, 100m, timeout);
            var paymentDecision = await paymentService
                .ExecuteNexusOperationAsync(s => s.CreateOrder(createOrderPaymentRequest));

            return paymentDecision;
        }
    }
}

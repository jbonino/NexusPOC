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
            var authAmount = 100m;

            var paymentDecision = await paymentService
                .ExecuteNexusOperationAsync(s => s.CreateOrder(new CreateOrderRequest(meijerOrderId, authAmount)));

            return paymentDecision;
        }
    }
}

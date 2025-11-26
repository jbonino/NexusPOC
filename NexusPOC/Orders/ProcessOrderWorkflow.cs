using NexusPOC.Payments;
using NexusPOC.Payments.Nexus;
using Temporalio.Workflows;

namespace NexusPOC.Orders
{
    [Workflow]
    public class ProcessOrderWorkflow
    {
        [WorkflowRun]
        public async Task<bool> RunAsync(string meijerOrderId)
        {
            Console.WriteLine("ProcessOrderWorkflow.RunAsync...");
            var paymentService = Workflow.CreateNexusClient<IPaymentsService>(IPaymentsService.EndpointName);

            var timeout = TimeSpan.FromSeconds(60);
            var authDecision = await paymentService
                .ExecuteNexusOperationAsync(s => s.CreateOrder(new CreateOrderRequest
                (
                    meijerOrderId, 
                    100m, 
                    timeout
                )));

            if (!authDecision.IsSuccess)
            {
                Console.WriteLine($"Authorization is not approved: {authDecision}");
                return false;
            }

            await Workflow.DelayAsync(TimeSpan.FromSeconds(1));

            var captureDecision = await paymentService
                .ExecuteNexusOperationAsync(s => s.FinalizeOrder(new FinalizeOrderRequest
                (
                    meijerOrderId, 
                    100m, 
                    timeout))
                );
            if (!captureDecision.IsSuccess)
            {
                Console.WriteLine($"Capture is not approved: {captureDecision}");
                return false;
            }

            await Workflow.DelayAsync(TimeSpan.FromSeconds(1));

            await paymentService
                .ExecuteNexusOperationAsync(s => s.SetFulfillmentStatus(new SetFulfillmentStatus
                (
                    meijerOrderId,
                    true,
                    timeout
                )));

            return true;
        }
    }
}

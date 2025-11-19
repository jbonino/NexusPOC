using NexusRpc;
using NexusRpc.Handlers;
using Temporalio.Nexus;

namespace NexusPOC.Payments.Nexus
{
    [NexusService]
    public interface IPaymentsService
    {
        /// <summary>
        /// The Nexus endpoint name for order payment operations.
        /// </summary>
        public const string EndpointName = "order-payments";

        /// <summary>
        /// Creates a new order with credit authorization.
        /// This is an asynchronous operation backed by PaymentNexusCreateOrderWorkflow.
        /// </summary>
        /// <returns>The order creation response with authorization details.</returns>
        [NexusOperation]
        CreateOrderResponse CreateOrder(CreateOrderRequest createOrderRequest);
    }

    [NexusServiceHandler(typeof(IPaymentsService))]
    public class PaymentService
    {
        [NexusOperationHandler]
        public static IOperationHandler<CreateOrderRequest, CreateOrderResponse> CreateOrder()
        {
            return WorkflowRunOperationHandler.FromHandleFactory((WorkflowRunOperationContext context, CreateOrderRequest request) =>
            {
                return context.StartWorkflowAsync(
                    (CreateOrderProxyWorkflow wf) => wf.RunAsync(request),
                    new()
                    {
                        Id = $"create-order-{context.HandlerContext.RequestId}",
                        TaskQueue = "payments",
                    });
            });
        }

    }
}

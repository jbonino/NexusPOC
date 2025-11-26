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
        public const string EndpointName = "payments-service";

        /// <summary>
        /// Creates a new order with credit authorization.
        /// This is an asynchronous operation backed by PaymentNexusCreateOrderWorkflow.
        /// </summary>
        /// <returns>The order creation response with authorization details.</returns>
        [NexusOperation]
        PaymentDecision CreateOrder(CreateOrderRequest request);

        /// <summary>
        /// Finalizes a order.
        /// </summary>
        /// <returns>The order creation response with payment details.</returns>
        [NexusOperation]
        PaymentDecision FinalizeOrder(FinalizeOrderRequest request);

        [NexusOperation]
        bool SetFulfillmentStatus(SetFulfillmentStatus request);
    }

    [NexusServiceHandler(typeof(IPaymentsService))]
    public class PaymentService
    {
        [NexusOperationHandler]
        public static IOperationHandler<CreateOrderRequest, PaymentDecision?> CreateOrder()
        {
            return WorkflowRunOperationHandler.FromHandleFactory((WorkflowRunOperationContext context, CreateOrderRequest request) =>
            {
                return context.StartWorkflowAsync(
                    (CreateOrderUpdateWorkflow wf) => wf.RunAsync(request),
                    new()
                    {
                        Id = $"{request.MeijerOrderId}-create-order",
                        TaskQueue = "payments",
                    });
            });
        }

        [NexusOperationHandler]
        public static IOperationHandler<FinalizeOrderRequest, PaymentDecision?> FinalizeOrder()
        {
            return WorkflowRunOperationHandler.FromHandleFactory((WorkflowRunOperationContext context, FinalizeOrderRequest request) =>
            {
                return context.StartWorkflowAsync(
                    (FinalizeOrderUpdateWorkflow wf) => wf.RunAsync(request),
                    new()
                    {
                        Id = $"{request.MeijerOrderId}-finalize-order",
                        TaskQueue = "payments",
                    });
            });
        }

        [NexusOperationHandler]
        public static IOperationHandler<SetFulfillmentStatus, bool> SetFulfillmentStatus()
        {
            return WorkflowRunOperationHandler.FromHandleFactory((WorkflowRunOperationContext context, SetFulfillmentStatus request) =>
            {
                return context.StartWorkflowAsync(
                    (SetFulfillmentStatusUpdateWorkflow wf) => wf.RunAsync(request),
                    new()
                    {
                        Id = $"{request.MeijerOrderId}-set-fulfillment-status",
                        TaskQueue = "payments",
                    });
            });
        }
    }
}

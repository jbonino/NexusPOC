using Temporalio.Workflows;

namespace NexusPOC.Payments
{
    public enum PaymentTypes
    {
        Authorize,
        Capture,
    }
    public record CreateOrderRequest(
        string MeijerOrderId,
        decimal Amount,
        TimeSpan StartToCloseTimeout
    );
    public record PaymentDecision(
        bool IsSuccess
    );
    public record PaymentTransaction(
        string OrderId,
        string TransactionId,
        decimal Amount,
        PaymentTypes Type,
        bool IsSuccess
    );

    [Workflow]
    public class ProcessPaymentWorkflow
    {
        private CreateOrderRequest? _createOrderRequest;
        private PaymentTransaction? _authorization;
        private PaymentTransaction? _capture;

        [WorkflowRun]
        public async Task<Dictionary<string, object>> RunAsync()
        {
            Console.WriteLine("ProcessPaymentWorkflow.RunAsync...");

            await Workflow.WaitConditionAsync(() => _createOrderRequest is not null);

            _authorization = await Workflow.ExecuteActivityAsync(
                (PaymentActivities a) => a.AuthorizeAsync(_createOrderRequest!.MeijerOrderId, _createOrderRequest.Amount),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            await Workflow.WaitConditionAsync(() => _capture is not null);
            await Workflow.WaitConditionAsync(() => Workflow.AllHandlersFinished);
            return new Dictionary<string, object>
            {
                { PaymentTypes.Authorize.ToString(), _authorization },
            };
        }

        [WorkflowUpdate]
        public async Task<PaymentDecision?> CreateOrder(CreateOrderRequest request)
        {
            Console.WriteLine("ProcessPaymentWorkflow.CreateOrder...");
            if (_createOrderRequest is null)
            {
                _createOrderRequest = request;
            }

            await Workflow.WaitConditionAsync(() => _authorization is not null);

            return new PaymentDecision(_authorization!.IsSuccess);
        }
    }
}

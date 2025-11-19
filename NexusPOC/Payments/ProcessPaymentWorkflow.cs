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
        decimal Amount
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
        private readonly Temporalio.Workflows.Mutex _orderCreate = new();
        [WorkflowRun]
        public async Task<Dictionary<string, object>> RunAsync()
        {
            Console.WriteLine("ProcessPaymentWorkflow.RunAsync...");

            await Workflow.WaitConditionAsync(() => _createOrderRequest is not null);

            _authorization = await Workflow.ExecuteActivityAsync(
                (PaymentActivities a) => a.AuthorizeAsync(_createOrderRequest!.MeijerOrderId, _createOrderRequest.Amount),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) });


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

            await _orderCreate.WaitOneAsync();
            if (_createOrderRequest is null)
            {
                _createOrderRequest = request;
            }
            _orderCreate.ReleaseMutex();

            await Workflow.WhenAnyAsync(
                Workflow.WaitConditionAsync(() => _authorization is not null),
                Workflow.DelayAsync(TimeSpan.FromMinutes(5))
            );
            return new PaymentDecision(_authorization is not null);
        }
    }
}

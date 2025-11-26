using Temporalio.Exceptions;
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
    public record FinalizeOrderRequest(
        string MeijerOrderId,
        decimal Amount,
        TimeSpan StartToCloseTimeout
    );
    public record SetFulfillmentStatus(
        string MeijerOrderId,
        bool IsFulfilled,
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
        private FinalizeOrderRequest? _finalizeOrderRequest;
        private PaymentTransaction? _authorization;
        private PaymentTransaction? _capture;
        private bool _isOrderFulfilled = false;

        [WorkflowRun]
        public async Task<Dictionary<string, object>> RunAsync()
        {
            Console.WriteLine("ProcessPaymentWorkflow.RunAsync...");
            while (_authorization is null || _authorization.IsSuccess is false)
            {
                _createOrderRequest = null;
                await Workflow.WaitConditionAsync(() => _createOrderRequest is not null);
                _authorization = await Workflow.ExecuteActivityAsync(
                    (PaymentActivities a) => a.AuthorizeAsync(_createOrderRequest!.MeijerOrderId, _createOrderRequest.Amount),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
            }

            while (_capture is null || _capture.IsSuccess is false)
            {
                _finalizeOrderRequest = null;
                await Workflow.WaitConditionAsync(() => _finalizeOrderRequest is not null);
                _capture = await Workflow.ExecuteActivityAsync(
                    (PaymentActivities a) => a.CaptureAsync(_finalizeOrderRequest!.MeijerOrderId, _finalizeOrderRequest.Amount, _authorization),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
            }

            await Workflow.WaitConditionAsync(() => _isOrderFulfilled);

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
            if (_createOrderRequest is not null)
            {
                throw new ApplicationFailureException("Processing CreateOrderRequest");
            }

            _createOrderRequest = request;
            await Workflow.WaitConditionAsync(() => _authorization is not null);

            return new PaymentDecision(_authorization!.IsSuccess);
        }

        [WorkflowUpdate]
        public async Task<PaymentDecision?> FinalizeOrder(FinalizeOrderRequest request)
        {
            Console.WriteLine("ProcessPaymentWorkflow.FinalizeORder...");
            if (_finalizeOrderRequest is not null)
            {
                throw new ApplicationFailureException("Processing FinalizeOrderRequest");
            }
            if (_authorization is null || !_authorization.IsSuccess)
            {
                throw new ApplicationFailureException("Authorization Required");
            }

            _finalizeOrderRequest = request;
            await Workflow.WaitConditionAsync(() => _capture is not null);

            return new PaymentDecision(_capture!.IsSuccess);
        }

        [WorkflowUpdate]
        public Task<bool> SetFulfillmentStatus(SetFulfillmentStatus request)
        {
            Console.WriteLine("ProcessPaymentWorkflow.SetFulfillmentStatus...");
            _isOrderFulfilled = request.IsFulfilled;
            return Task.FromResult(true);
        }

        [WorkflowQuery]
        public PaymentTransaction? GetAuthorization()
        {
            return _authorization;
        }

        [WorkflowQuery]
        public PaymentTransaction? GetCapture()
        {
            return _capture;
        }

        [WorkflowQuery]
        public bool IsCompleted()
        {
            var completedAuth = _authorization?.IsSuccess ?? false;
            var completedCapture = _capture?.IsSuccess ?? false;

            return completedAuth && completedCapture && _isOrderFulfilled;
        }
    }
}

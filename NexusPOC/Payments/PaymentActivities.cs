using Temporalio.Activities;

namespace NexusPOC.Payments
{
    public class PaymentActivities
    {
        [Activity]
        public async Task<PaymentTransaction> AuthorizeAsync(string orderId, decimal amount)
        {
            Console.WriteLine($"Authorizing card for order {orderId}...");
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Authorize completed for order {orderId}");
            return new PaymentTransaction(orderId, Guid.NewGuid().ToString(), amount, PaymentTypes.Authorize, true);
        }

    }
}

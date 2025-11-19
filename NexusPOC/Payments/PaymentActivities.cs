using Temporalio.Activities;

namespace NexusPOC.Payments
{
    public class PaymentActivities
    {
        [Activity]
        public async Task<PaymentTransaction> AuthorizeAsync(string orderId, decimal amount)
        {
            Console.WriteLine($"Authorizing card for order {orderId}...");

            var delay = TimeSpan.FromSeconds(15);
            Console.WriteLine($"Task.Delay({delay}) to simulate authorization");
            await Task.Delay(delay);

            Console.WriteLine($"Authorize completed for order {orderId}");
            return new PaymentTransaction(orderId, Guid.NewGuid().ToString(), amount, PaymentTypes.Authorize, true);
        }

    }
}

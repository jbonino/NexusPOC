using NexusPOC.Orders;
using NexusPOC.Payments;
using NexusPOC.Payments.Nexus;
using Temporalio.Client;
using Temporalio.Worker;

namespace NexusPOC;

static class Program
{
    /* CMDS
temporal server start-dev

temporal operator namespace create --namespace payments
temporal operator namespace create --namespace orders
temporal operator nexus endpoint create --name order-payments --target-namespace payments --target-task-queue payments
     */

    static async Task Main()
    {
        var paymentsClient = await TemporalClient.ConnectAsync(new("localhost:7233") { Namespace = "payments" });
        var ordersClient = await TemporalClient.ConnectAsync(new("localhost:7233") { Namespace = "orders" });
        using var tokenSource = new CancellationTokenSource();

        // Start workers
        using var paymentWorker = new TemporalWorker(
            paymentsClient,
            new TemporalWorkerOptions(taskQueue: "payments")
                .AddWorkflow<ProcessPaymentWorkflow>()
                .AddWorkflow<CreateOrderUpdateWorkflow>()
                .AddAllActivities(new PaymentActivities())
                .AddAllActivities(new UpdateProcessPaymentActivities(paymentsClient))
                .AddNexusService(new PaymentService())
        );
        using var ordersWorker = new TemporalWorker(
            ordersClient,
            new TemporalWorkerOptions(taskQueue: "orders")
                .AddWorkflow<ProcessOrderWorkflow>()
        );
        _ = RunWorker(paymentWorker, tokenSource);
        _ = RunWorker(ordersWorker, tokenSource);
        Console.WriteLine("Running workers");

        // Start an order workflow
        var random = new Random();
        var meijerOrderId = $"meijer-{random.Next(10000, 100000)}";
        // payment
        await paymentsClient.StartWorkflowAsync(
            (ProcessPaymentWorkflow wf) => wf.RunAsync(),
            new(id: meijerOrderId, taskQueue: "payments"));
        // order
        var result = await ordersClient.ExecuteWorkflowAsync(
            (ProcessOrderWorkflow wf) => wf.RunAsync(meijerOrderId),
            new(id: meijerOrderId, taskQueue: "orders"));
        Console.WriteLine(result.ToString());
        Console.ReadKey();
    }

    static Task RunWorker(TemporalWorker worker, CancellationTokenSource tokenSource)
    {
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            tokenSource.Cancel();
            eventArgs.Cancel = true;
        };
        return worker.ExecuteAsync(tokenSource.Token);
    }
}

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A proof-of-concept demonstrating [Temporal Nexus](https://docs.temporal.io/nexus) — cross-namespace communication between two independent Temporal namespaces (`orders` and `payments`) via a Nexus service, without either namespace's workflows directly depending on the other's workflow types.

## Commands

Requires the Temporal CLI and a locally running dev server plus one-time namespace/endpoint setup (see comment block at the top of `NexusPOC/Program.cs`):

```
temporal server start-dev

temporal operator namespace create --namespace payments
temporal operator namespace create --namespace orders
temporal operator nexus endpoint create --name payments-service --target-namespace payments --target-task-queue payments
```

Then build/run the .NET app (net8.0):

```
dotnet build
dotnet run --project NexusPOC
```

`Program.cs` is both the worker host and a driver script: it starts the `payments` and `orders` workers, then immediately kicks off one demo order (`ProcessOrderWorkflow`) and one payment saga (`ProcessPaymentWorkflow`) against a randomly generated `meijer-<id>` order id, printing progress to the console.

There is no test project in this repo currently.

## Architecture

Two namespaces connected only through a Nexus endpoint:

- **`orders`** — `ProcessOrderWorkflow` is the caller. It never touches the `payments` client/workflows directly; it calls `Workflow.CreateNexusClient<IPaymentsService>(...)` and invokes Nexus operations (`CreateOrder` → `FinalizeOrder` → `SetFulfillmentStatus`) in sequence.
- **`payments`** — hosts `ProcessPaymentWorkflow` (the long-lived per-order saga, started up front by `Program.cs` and driven via Workflow Updates), the `[NexusServiceHandler]` `PaymentService`, three thin `*UpdateWorkflow` classes (one per operation), and `PaymentActivities`/`UpdateProcessPaymentActivities` (the actual work + the Update-forwarding).

Each Nexus operation is backed by `WorkflowRunOperationHandler.FromHandleFactory`, which starts the matching `*UpdateWorkflow` instead of handling the call inline — this makes the operation async/awaitable and independently trackable. That workflow just runs one activity in `UpdateProcessPaymentActivities`, which looks up the running `ProcessPaymentWorkflow` by order id (`GetWorkflowHandle`) and sends it the real Workflow Update (`RetryPolicy.MaximumAttempts = 1`, since Updates aren't safe to retry blindly).

Call flow: `ProcessOrderWorkflow` → Nexus operation → `*UpdateWorkflow` → Workflow Update on `ProcessPaymentWorkflow` → `PaymentActivities`, with the result flowing back up the same chain.

The order id (`MeijerOrderId`) is the correlation key throughout: it's the `ProcessPaymentWorkflow`'s workflow ID, part of each `*UpdateWorkflow`'s ID (`{orderId}-create-order`, etc.), and how `UpdateProcessPaymentActivities` finds the workflow to update.

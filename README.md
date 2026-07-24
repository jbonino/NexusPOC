# NexusPOC

A proof-of-concept demonstrating [Temporal Nexus](https://docs.temporal.io/nexus) — cross-namespace communication between two independent Temporal namespaces (`orders` and `payments`), without either namespace's workflows directly depending on the other's workflow types.

## Overview

- **`orders` namespace** runs `ProcessOrderWorkflow`, which drives an order through authorization, capture, and fulfillment by calling a Nexus service — it never talks to the `payments` namespace or its workflows directly.
- **`payments` namespace** hosts `ProcessPaymentWorkflow`, a long-lived saga for a single order that is driven via Workflow Updates sent from the Nexus side, plus the Nexus service handler and the short-lived "update workflows" that back each Nexus operation.

See [CLAUDE.md](./CLAUDE.md) for a detailed breakdown of the architecture and call flow.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Temporal CLI](https://docs.temporal.io/cli)

## Setup

Start a local Temporal dev server and create the namespaces and Nexus endpoint:

```sh
temporal server start-dev

temporal operator namespace create --namespace payments
temporal operator namespace create --namespace orders
temporal operator nexus endpoint create --name payments-service --target-namespace payments --target-task-queue payments
```

## Running

```sh
dotnet build
dotnet run --project NexusPOC
```

This starts the `payments` and `orders` workers, then kicks off a single demo order (a randomly generated `meijer-<id>`) through the full authorize → capture → fulfill flow, printing progress to the console.

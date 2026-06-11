# System Sync Engine

A production-style .NET Worker Service prototype for synchronizing business records between systems safely and reliably.

This project demonstrates a backend integration pattern commonly needed when businesses need to move data between CRMs, ERPs, billing platforms, vendor APIs, spreadsheets, and internal databases.

The focus is not just moving data. The focus is moving data in a way that is repeatable, auditable, retry-safe, and resistant to duplicate processing.

---

## Business Problem

Many small and mid-sized businesses rely on disconnected systems:

* CRM platforms
* ERP systems
* QuickBooks or accounting tools
* Vendor APIs
* Internal databases
* Spreadsheets
* Order management systems
* Field service platforms

A common problem is keeping records synchronized without creating duplicates, losing failed records, or requiring manual cleanup after an error.

This prototype shows how I approach that class of problem.

---

## What This Demonstrates

System Sync Engine demonstrates a reliable sync pipeline that:

* Pulls changed records from a source system
* Validates and normalizes data before writing
* Performs idempotent upserts into a destination database
* Saves checkpoints so the same records are not repeatedly processed
* Tracks each sync run for auditability
* Captures failed or skipped records for later review
* Uses configurable retry behavior
* Separates source, destination, sync state, and error tracking concerns

This pattern can be adapted to many real-world integration projects, including:

* API-to-database integrations
* CRM-to-accounting sync
* vendor order imports
* QuickBooks-style billing sync
* scheduled ETL jobs
* customer/order/invoice synchronization
* operational data pipelines
* background business automation jobs

---

## Example Use Cases

This architecture could be adapted for scenarios such as:

* Pull new orders from a vendor API and write them into an internal database
* Sync customer records from a CRM into a billing platform
* Import spreadsheet or CSV data, validate it, and upsert it into a database
* Pull changed invoices or payments from one system and reconcile them with another
* Run a scheduled background job that processes only records changed since the last successful sync
* Capture failed records without stopping the entire job

---

## Tech Stack

* .NET 10 Worker Service
* C#
* SQLite
* Microsoft.Extensions.Hosting
* Microsoft.Extensions.Configuration
* Microsoft.Extensions.Logging

SQLite is used for the prototype so the project is easy to run locally. The same pattern could be adapted to SQL Server, PostgreSQL, MySQL, or a cloud database.

---

## Core Features

* Source system abstraction
* Destination repository abstraction
* Changed-record processing
* Validation before persistence
* Data normalization
* Idempotent upsert behavior
* Checkpoint-based sync state
* Configurable retry behavior
* Failed-record tracking
* Durable sync run history
* Structured logging
* Runtime configuration through `appsettings.json`

---

## Current Flow

```text
Source System
    ↓
CustomerSyncService
    ↓
Validation / Normalization
    ↓
Retry Policy
    ↓
Destination Repository
    ↓
Sync State / Sync Errors / Sync Run History
```

---

## How to Run

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run --project SystemSyncEngine.Worker
```

The worker service will run locally and demonstrate the sync process using the prototype source and SQLite destination.

---

## Data Safety Pattern

The sync process is designed around several reliability principles.

### Idempotency

Records are upserted rather than blindly inserted. This helps prevent duplicate destination records when a job is rerun.

### Checkpointing

The sync engine stores the last successful sync point so future runs can process only changed records.

### Validation

Records are validated before persistence. Invalid records are skipped and logged rather than silently corrupting the destination.

### Error Capture

Failed records are stored in a `SyncErrors` table for review instead of disappearing into application logs.

### Audit History

Each sync run is captured in a `SyncRuns` table so there is a durable history of what happened.

---

## Prototype Scope

This is intentionally a prototype, not a packaged commercial product.

The goal is to demonstrate the architecture and coding pattern behind a reliable sync service.

Current implementation uses:

* a fake source system client
* a SQLite destination
* customer records as the sample domain

In a real project, the fake source client could be replaced with an API client for systems such as QuickBooks, Zoom, HubSpot, Salesforce, Stripe, a vendor portal, or a custom internal API.

---

## Why This Matters

Many integrations fail because they only handle the happy path.

A reliable business sync process needs to answer questions like:

* What happens if the same record is processed twice?
* What happens if a record fails validation?
* What happens if an API call succeeds but the job crashes afterward?
* How do we know what ran, what succeeded, and what failed?
* How do we resume safely without manual cleanup?
* How do we troubleshoot a failed sync later?

This project demonstrates the foundation for answering those questions.

---

## Potential Extensions

Future improvements could include:

* SQL Server or PostgreSQL destination support
* external REST API source client
* OAuth-based API authentication
* webhook-triggered sync
* background scheduling
* retry queue for failed records
* admin dashboard for sync runs and errors
* Docker deployment
* cloud deployment to Azure or AWS
* automated tests
* structured JSON logging
* alerting on failed sync runs

---

## Repository Purpose

This repository is intended as a portfolio-quality technical prototype showing how I structure backend integration and data synchronization work.

It is most relevant to projects involving:

* API integrations
* backend automation
* data sync
* ETL pipelines
* database upserts
* business system connectors
* scheduled background jobs
* operational reliability improvements

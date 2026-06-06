# System Sync Engine

A production-style .NET Worker Service prototype that syncs customer data from a source system into a relational database with retries, validation, checkpointing, audit history, and failed-record tracking.

## Business Use Case

Many businesses need to move data between systems such as CRMs, ERPs, billing platforms, vendor APIs, spreadsheets, and internal databases.

This prototype demonstrates a reliable backend sync pattern for:

- API-to-database integrations
- CRM/customer data synchronization
- ETL-style business automation
- scheduled background data jobs
- failed-record auditing and retry-safe processing

## Tech Stack

- .NET 10 Worker Service
- C#
- SQLite
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging

## Features

- Pulls updated customer records from a source system abstraction
- Validates required fields before writing
- Normalizes data before persistence
- Performs idempotent upserts into SQLite
- Saves sync checkpoints to avoid duplicate processing
- Stores failed/skipped records in a SyncErrors table
- Stores durable run history in a SyncRuns table
- Uses configurable retry behavior
- Uses appsettings.json for runtime configuration

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
SQLite Destination Repository
    ↓
Sync State / Sync Errors / Sync Run History
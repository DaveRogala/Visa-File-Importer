# VCF Feed Import

A .NET 10 batch ETL console application that monitors a directory for Visa Card Feed (VCF) CSV files, parses them, and imports the transaction records into a SQL Server database.

## Overview

The application is designed to run as a scheduled job (Windows Task Scheduler, cron, etc.). On each run it:

1. Scans the configured input directory for `*.csv` files
2. Skips any file whose name already exists in the database (deduplication)
3. Parses each new file using CsvHelper via MagellanFileServices, with unescaped-quote correction enabled
4. Maps every row to a `VcfTransaction` entity, collecting row-level errors without stopping the batch
5. Persists the import record and all successfully parsed transactions to SQL Server
6. Moves the file to a `processed/` subfolder on success or an `errors/` subfolder on failure, writing a timestamped error log when needed

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019 or later (Windows Authentication supported)
- Read/write access to the input directory and its `processed/` and `errors/` subfolders

## Configuration

Each environment has its own `appsettings.<env>.json`. The active file is selected by the `DOTNET_ENVIRONMENT` environment variable (defaults to `local`).

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Data:BasePath` | Full path to the directory containing incoming VCF CSV files |

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=MY_SERVER;Database=AribaReporting;Trusted_Connection=yes;TrustServerCertificate=True;"
  },
  "Data": {
    "BasePath": "H:\\Transfer\\In\\M4G\\VCF"
  }
}
```

## Database Setup

Apply all pending migrations to create or update the schema:

```bash
dotnet ef database update
```

To add a migration after a model change:

```bash
dotnet ef migrations add <MigrationName>
```

The schema lives in the `vcf` SQL Server schema. Three tables are created:

| Table | Purpose |
|---|---|
| `vcf.ImportFiles` | One row per imported file; `ImportFileName` has a unique index |
| `vcf.VcfTransactions` | One row per parsed transaction |
| `vcf.ImportFileVCFTransactions` | Junction linking files to their transactions |

## Running

```bash
dotnet run
```

The process exits with code 0 on success and throws a `VcfProcessingException` (non-zero exit) if any file encountered errors. Check the `errors/` subfolder under `BasePath` for timestamped logs.

## Running Tests

```bash
dotnet test VCFFileImport.Tests/VCFFileImport.Tests.csproj
```

If the test project has not yet been added to the solution:

```bash
dotnet sln add VCFFileImport.Tests/VCFFileImport.Tests.csproj
dotnet test
```

## CSV File Format

Files must be comma-delimited UTF-8 with a header row matching the following column names exactly:

| Column | Type | Notes |
|---|---|---|
| `Account Number_ Mask first 10 Digits` | string | First 10 digits replaced with `*`; remainder parsed as the last-six integer |
| `Account Open Date` | string | `MMddyyyy` format |
| `Account Close Date` | string | `MMddyyyy` format; optional |
| `Billing Amount` | decimal | |
| `Transaction Date` | string | `MMddyyyy` format |
| `Posting Date` | string | `MMddyyyy` format |
| `Transaction Type Code` | string | Max 10 chars |
| `Commodity Code` | string | Optional |
| `First Name` | string | |
| `Last Name` | string | |
| `Employee ID` | string | Max 8 chars |
| `Transaction Reference Number` | string | |
| `Last Credit Limit Change Date` | string | `MMddyyyy` format; optional |
| `Status Code` | integer | |
| `Status Date` | string | `MMddyyyy` format |
| `Supplier Name` | string | Optional; unescaped quotes are corrected automatically |
| `Merchant Category Code` | integer | |
| `Purchase Identification` | string | Optional |
| `Cardholder Transaction Approval` | string | `Y` = approved; any other value = not approved |
| `Supplier City` | string | Optional |
| `Supplier State` | string | Optional |
| `Supplier Zip Code` | string | Optional |
| `Company Name` | string | Optional |

## Project Structure

```
VCFFileImport/
├── Contracts/          # IVcfServices interface
├── Data/
│   ├── Configurations/ # IEntityTypeConfiguration<T> classes (Fluent API)
│   └── VcfContext.cs
├── Exceptions/         # VcfConfigurationException, VcfProcessingException
├── Migrations/         # EF Core migration history
├── Models/
│   ├── Database/       # ImportFile, VcfTransaction, ImportFileVcfTransaction
│   └── DTOs/           # VcfTransactionDto (CsvHelper mapping)
├── Repositories/       # ImportFileRepository
├── Services/           # VcfServices (core processing logic)
├── Program.cs
└── appsettings*.json

VCFFileImport.Tests/
└── VcfServicesTests.cs # xUnit tests for VcfServices
```

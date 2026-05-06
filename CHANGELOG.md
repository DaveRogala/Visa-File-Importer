# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-05-06

### Added
- `IEntityTypeConfiguration<T>` classes (`Data/Configurations/`) for `ImportFile`, `VcfTransaction`, and `ImportFileVcfTransaction` — all EF Core schema configuration is now in Fluent API
- Unique index on `ImportFileName` enforced at the database level, complementing the existing application-layer duplicate check
- `VcfConfigurationException` (extends `InvalidOperationException`) thrown when `BasePath` is missing or whitespace
- `VcfProcessingException` thrown when `ProcessFileAsync` returns false, giving callers a typed signal for data/file-level failures
- `VCFFileImport.Tests` unit test project covering 17 scenarios: constructor validation, duplicate filename handling, unescaped quotes, single broken rows, malformed headers, nullable date mapping, optional field null-coercion, `CardholderTransactionApproval` case sensitivity, account number mask parsing, and archive filename format

### Changed
- All six date-only fields on `VcfTransaction` (`AccountOpenDate`, `AccountCloseDate`, `TransactionDate`, `PostingDate`, `LastCreditLimitChangeDate`, `StatusDate`) changed from `DateTime`/`DateTime?` to `DateOnly`/`DateOnly?`, mapping to the SQL Server `date` column type
- `GetDateFromString` updated to return `DateOnly?` using `DateOnly.TryParseExact`
- `AccountNumberLastSix` parsing changed from `int.TryParse` (ignored return value, silent zero on failure) to `int.Parse` (throws on malformed input, consistent with other numeric fields)
- All data annotation attributes (`[Table]`, `[Column]`, `[Key]`, `[Index]`, `[MaxLength]`, `[Precision]`, `[ForeignKey]`) removed from model classes in favour of Fluent API configuration
- `TransactionReferenceNumber` index changed from unique to non-unique
- Generic `Exception` throws replaced with typed `VcfConfigurationException` and `VcfProcessingException`
- `ImportFileVcfTransaction` navigation properties initialised with `null!` to satisfy nullable reference type analysis while preserving EF Core's population behaviour

### Migration required
A new EF Core migration is needed before deploying 1.0.0 to apply:
- Column type changes from `datetime2` to `date` for all six date fields
- Addition of the unique constraint on `vcf.ImportFiles.ImportFileName`
- Removal of the unique constraint from `vcf.VcfTransactions.TransactionReferenceNumber`

```bash
dotnet ef migrations add v1_0_0_SchemaUpdates
dotnet ef database update
```

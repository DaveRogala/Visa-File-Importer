using GenericRepositories.Interfaces;
using MagellanFileServices.Contracts;
using MagellanFileServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using System.Text;
using VCFFileImport.Data;
using VCFFileImport.Exceptions;
using VCFFileImport.Models.Database;
using VCFFileImport.Models.DTOs;
using VCFFileImport.Services;

namespace VCFFileImport.Tests;

// ASSUMPTIONS:
//   1. ObjectResult<T> has a public parameterless constructor and settable
//      ObjectResults / Errors properties (object-initialiser syntax used throughout).
//   2. IGenericRepository.AddAsync returns Task. If it returns Task<TEntity>,
//      change Returns(Task.CompletedTask) to ReturnsAsync(new ImportFile()).

public class VcfServicesTests : IDisposable
{
    private readonly Mock<IGenericRepository<ImportFile, VcfContext, int>> _repoMock;
    private readonly Mock<IFileServices> _fileServicesMock;
    private readonly string _tempDir;

    public VcfServicesTests()
    {
        _repoMock         = new Mock<IGenericRepository<ImportFile, VcfContext, int>>();
        _fileServicesMock = new Mock<IFileServices>();
        _tempDir          = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _repoMock
            .Setup(r => r.FindFirstAsync(
                It.IsAny<Expression<Func<ImportFile, bool>>>(),
                It.IsAny<Func<IQueryable<ImportFile>, IOrderedQueryable<ImportFile>>>()))
            .ReturnsAsync((ImportFile?)null);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<ImportFile>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private VcfServices BuildSut() =>
        new(_repoMock.Object,
            NullLogger<VcfServices>.Instance,
            BuildConfig(_tempDir),
            _fileServicesMock.Object);

    private static IConfiguration BuildConfig(string basePath) =>
        new ConfigurationBuilder()
            .Add(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string?> { ["Data:BasePath"] = basePath }
            })
            .Build();

    // "**********123456" → Replace("*","") → "123456" → int.Parse → 123456
    private static VcfTransactionDto ValidDto(
        string supplierName       = "Test Supplier",
        string billingAmount      = "99.99",
        string cardholderApproval = "Y") => new()
    {
        AccountNumberMaskfirst10Digits = "**********123456",
        AccountOpenDate               = "01152020",
        AccountCloseDate              = "",
        BillingAmount                 = billingAmount,
        TransactionDate               = "03102024",
        PostingDate                   = "03112024",
        TransactionTypeCode           = "P",
        CommodityCode                 = "",
        FirstName                     = "Jane",
        LastName                      = "Doe",
        EmployeeID                    = "EMP001",
        TransactionReferenceNumber    = "REF0001",
        LastCreditLimitChangeDate     = "",
        StatusCode                    = "1",
        StatusDate                    = "03112024",
        SupplierName                  = supplierName,
        MerchantCategoryCode          = "5411",
        PurchaseIdentification        = "PUR001",
        CardholderTransactionApproval = cardholderApproval,
        SupplierCity                  = "Austin",
        SupplierState                 = "TX",
        SupplierZipCode               = "78701",
        CompanyName                   = "Acme Corp"
    };

    private void SetupGetData(IEnumerable<VcfTransactionDto> rows, List<string>? parseErrors = null) =>
        _fileServicesMock
            .Setup(f => f.GetDataFromFile<VcfTransactionDto>(
                It.IsAny<string>(),
                It.Is<Encoding>(e => e == Encoding.UTF8),
                0,
                true))
            .Returns(new ObjectResult<VcfTransactionDto>
            {
                ObjectResults = rows.ToList(),
                Errors        = parseErrors ?? []
            });

    private string WriteCsvFile(string name = "transactions.csv")
    {
        string path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, "placeholder");
        return path;
    }

    private void CaptureAddAsync(ref VcfTransaction? captured)
    {
        VcfTransaction? local = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => local = f.ImportFileVcfTransactions.FirstOrDefault()?.VcfTransaction)
            .Returns(Task.CompletedTask);
        captured = local;
    }

    // -----------------------------------------------------------------------
    // Constructor
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullOrMissingBasePath_ThrowsVcfConfigurationException()
    {
        IConfiguration empty = new ConfigurationBuilder()
            .Add(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string?> { ["Other:Key"] = "x" }
            })
            .Build();

        Assert.Throws<VcfConfigurationException>(() =>
            new VcfServices(_repoMock.Object, NullLogger<VcfServices>.Instance, empty, _fileServicesMock.Object));
    }

    [Fact]
    public void Constructor_WhitespaceBasePath_ThrowsVcfConfigurationException()
    {
        IConfiguration cfg = new ConfigurationBuilder()
            .Add(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string?> { ["Data:BasePath"] = "   " }
            })
            .Build();

        Assert.Throws<VcfConfigurationException>(() =>
            new VcfServices(_repoMock.Object, NullLogger<VcfServices>.Instance, cfg, _fileServicesMock.Object));
    }

    // -----------------------------------------------------------------------
    // Empty / non-CSV directory
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_EmptyDirectory_ReturnsTrueWithNoInteractions()
    {
        using VcfServices sut = BuildSut();

        bool result = await sut.ProcessFileAsync();

        Assert.True(result);
        _fileServicesMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessFileAsync_NonCsvFilesOnly_ReturnsTrueWithNoInteractions()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.txt"),    "ignore");
        File.WriteAllText(Path.Combine(_tempDir, "report.xlsx"), "ignore");

        using VcfServices sut = BuildSut();

        bool result = await sut.ProcessFileAsync();

        Assert.True(result);
        _fileServicesMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessFileAsync_MixedDirectory_OnlyCsvFilesProcessed()
    {
        WriteCsvFile("data.csv");
        File.WriteAllText(Path.Combine(_tempDir, "readme.txt"),  "ignore");
        File.WriteAllText(Path.Combine(_tempDir, "archive.zip"), "ignore");

        SetupGetData([ValidDto()]);
        using VcfServices sut = BuildSut();

        await sut.ProcessFileAsync();

        _fileServicesMock.Verify(
            f => f.GetDataFromFile<VcfTransactionDto>(
                It.IsAny<string>(), It.IsAny<Encoding>(), It.IsAny<int>(), It.IsAny<bool>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // Happy path
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_SingleValidFile_ReturnsTrueAndCallsHandleFileSuccess()
    {
        WriteCsvFile("good.csv");
        SetupGetData([ValidDto()]);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.True(result);
        _fileServicesMock.Verify(
            f => f.HandleFileSuccess(_tempDir, "good.csv", It.IsAny<string>()),
            Times.Once);
        _fileServicesMock.Verify(
            f => f.HandleFileError(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _fileServicesMock.Verify(
            f => f.HandleFileError(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<List<string>>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessFileAsync_MultipleValidFiles_AllProcessedSuccessfully()
    {
        WriteCsvFile("file1.csv");
        WriteCsvFile("file2.csv");
        WriteCsvFile("file3.csv");
        SetupGetData([ValidDto()]);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.True(result);
        _fileServicesMock.Verify(
            f => f.HandleFileSuccess(_tempDir, It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(3));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<ImportFile>()), Times.Exactly(3));
    }

    // -----------------------------------------------------------------------
    // Duplicate filename
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_DuplicateFile_ReturnsFalseAndCallsHandleFileError()
    {
        WriteCsvFile("already.csv");

        _repoMock
            .Setup(r => r.FindFirstAsync(
                It.IsAny<Expression<Func<ImportFile, bool>>>(),
                It.IsAny<Func<IQueryable<ImportFile>, IOrderedQueryable<ImportFile>>>()))
            .ReturnsAsync(new ImportFile { ImportFileName = "already.csv", ArchiveFileName = "already.csv" });

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.False(result);
        _fileServicesMock.Verify(
            f => f.HandleFileError(
                _tempDir,
                "already.csv",
                It.Is<string>(m => m.Contains("already imported.")),
                It.IsAny<string>()),
            Times.Once);
        _fileServicesMock.Verify(
            f => f.HandleFileSuccess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _fileServicesMock.Verify(
            f => f.GetDataFromFile<VcfTransactionDto>(
                It.IsAny<string>(), It.IsAny<Encoding>(), It.IsAny<int>(), It.IsAny<bool>()),
            Times.Never);
    }

    // -----------------------------------------------------------------------
    // Unescaped quotes in SupplierName
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_GetDataFromFile_AlwaysPassesFixUnescapedQuotesTrue()
    {
        string expectedPath = Path.Combine(_tempDir, "quotes.csv");
        WriteCsvFile("quotes.csv");
        SetupGetData([ValidDto()]);

        using VcfServices sut = BuildSut();
        await sut.ProcessFileAsync();

        _fileServicesMock.Verify(
            f => f.GetDataFromFile<VcfTransactionDto>(expectedPath, Encoding.UTF8, 0, true),
            Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_SupplierNameWithQuotes_IsMappedAndSavedCorrectly()
    {
        const string supplierWithQuotes = "O\"Reilly Auto Parts";
        WriteCsvFile("quotes.csv");
        SetupGetData([ValidDto(supplierName: supplierWithQuotes)]);

        VcfTransaction? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => captured = f.ImportFileVcfTransactions.First().VcfTransaction)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.True(result);
        Assert.NotNull(captured);
        Assert.Equal(supplierWithQuotes, captured!.SupplierName);
    }

    // -----------------------------------------------------------------------
    // Single broken line — all others should succeed
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_OneBadRow_SkipsBadRowSavesRemainingAndReturnsFalse()
    {
        WriteCsvFile("partial.csv");

        VcfTransactionDto bad = ValidDto() with { BillingAmount = "NOT_A_NUMBER" };
        SetupGetData([ValidDto(), bad, ValidDto()]);

        ImportFile? savedFile = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => savedFile = f)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.False(result);
        Assert.NotNull(savedFile);
        Assert.Equal(2, savedFile!.ImportFileVcfTransactions.Count);
        _fileServicesMock.Verify(
            f => f.HandleFileError(
                _tempDir, "partial.csv", It.IsAny<string>(), It.IsAny<string>(),
                It.Is<List<string>>(errs => errs.Count >= 1)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_AllRowsBad_SavesEmptyTransactionListAndReturnsFalse()
    {
        WriteCsvFile("all_bad.csv");

        SetupGetData([
            ValidDto() with { BillingAmount = "INVALID" },
            ValidDto() with { StatusCode    = "INVALID" }
        ]);

        ImportFile? savedFile = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => savedFile = f)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.False(result);
        Assert.NotNull(savedFile);
        Assert.Empty(savedFile!.ImportFileVcfTransactions);
    }

    // -----------------------------------------------------------------------
    // Malformed headers — file service returns parse errors
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_FileServiceParseErrors_ReturnsFalseAndReportsErrors()
    {
        WriteCsvFile("bad_headers.csv");

        _fileServicesMock
            .Setup(f => f.GetDataFromFile<VcfTransactionDto>(
                It.IsAny<string>(), It.Is<Encoding>(e => e == Encoding.UTF8), 0, true))
            .Returns(new ObjectResult<VcfTransactionDto>
            {
                ObjectResults = [],
                Errors        = ["Header mismatch on column 3", "Header mismatch on column 7"]
            });

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.False(result);
        _fileServicesMock.Verify(
            f => f.HandleFileError(
                _tempDir, "bad_headers.csv", It.IsAny<string>(), It.IsAny<string>(),
                It.Is<List<string>>(errs => errs.Any(e => e.Contains("Header mismatch")))),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // Invalid required date field — row is skipped
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_InvalidRequiredDate_SkipsRowAndReportsError()
    {
        WriteCsvFile("bad_date.csv");

        // Empty TransactionDate → GetDateFromString returns null → (DateOnly)null throws
        SetupGetData([ValidDto() with { TransactionDate = "" }]);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.False(result);
        _fileServicesMock.Verify(
            f => f.HandleFileError(
                _tempDir, "bad_date.csv", It.IsAny<string>(), It.IsAny<string>(),
                It.Is<List<string>>(errs => errs.Count >= 1)),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // Nullable date fields
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_NullableDatesEmpty_MappedToNull()
    {
        WriteCsvFile("nullable.csv");
        SetupGetData([ValidDto() with { AccountCloseDate = "", LastCreditLimitChangeDate = "" }]);

        VcfTransaction? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => captured = f.ImportFileVcfTransactions.First().VcfTransaction)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.True(result);
        Assert.NotNull(captured);
        Assert.Null(captured!.AccountCloseDate);
        Assert.Null(captured.LastCreditLimitChangeDate);
    }

    [Fact]
    public async Task ProcessFileAsync_NullableDatesPopulated_MappedCorrectly()
    {
        WriteCsvFile("dates.csv");
        SetupGetData([ValidDto() with
        {
            AccountCloseDate          = "12312025",
            LastCreditLimitChangeDate = "06152024"
        }]);

        VcfTransaction? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => captured = f.ImportFileVcfTransactions.First().VcfTransaction)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        await sut.ProcessFileAsync();

        Assert.NotNull(captured);
        Assert.Equal(new DateOnly(2025, 12, 31), captured!.AccountCloseDate);
        Assert.Equal(new DateOnly(2024, 6, 15),  captured.LastCreditLimitChangeDate);
    }

    // -----------------------------------------------------------------------
    // Optional string fields — whitespace → null
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_WhitespaceOptionalStrings_MappedToNull()
    {
        WriteCsvFile("whitespace.csv");
        SetupGetData([ValidDto() with
        {
            CommodityCode          = "   ",
            SupplierName           = "   ",
            PurchaseIdentification = "   ",
            SupplierCity           = "   ",
            SupplierState          = "   ",
            SupplierZipCode        = "   ",
            CompanyName            = "   "
        }]);

        VcfTransaction? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => captured = f.ImportFileVcfTransactions.First().VcfTransaction)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.True(result);
        Assert.NotNull(captured);
        Assert.Null(captured!.CommodityCode);
        Assert.Null(captured.SupplierName);
        Assert.Null(captured.PurchaseIdentification);
        Assert.Null(captured.SupplierCity);
        Assert.Null(captured.SupplierState);
        Assert.Null(captured.SupplierZipCode);
        Assert.Null(captured.CompanyName);
    }

    // -----------------------------------------------------------------------
    // CardholderTransactionApproval mapping
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Y",   true)]
    [InlineData("N",   false)]
    [InlineData("",    false)]
    [InlineData("y",   false)]   // case-sensitive
    [InlineData("YES", false)]
    public async Task ProcessFileAsync_CardholderApproval_MapsCorrectly(string raw, bool expected)
    {
        WriteCsvFile($"approval.csv");
        SetupGetData([ValidDto(cardholderApproval: raw)]);

        VcfTransaction? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => captured = f.ImportFileVcfTransactions.First().VcfTransaction)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        await sut.ProcessFileAsync();

        Assert.NotNull(captured);
        Assert.Equal(expected, captured!.CardholderTransactionApproval);
    }

    // -----------------------------------------------------------------------
    // Account number mask parsing
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_AccountMask_StripsAsterisksAndParsesLastSixDigits()
    {
        WriteCsvFile("mask.csv");
        SetupGetData([ValidDto()]);  // uses "**********123456"

        VcfTransaction? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => captured = f.ImportFileVcfTransactions.First().VcfTransaction)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        await sut.ProcessFileAsync();

        Assert.NotNull(captured);
        Assert.Equal(123456, captured!.AccountNumberLastSix);
        Assert.Equal("**********123456", captured.AccountNumberMaskfirst10Digits);
    }

    [Fact]
    public async Task ProcessFileAsync_AllAsterisksInMask_RowSkippedAndErrorReported()
    {
        WriteCsvFile("mask_bad.csv");
        // All asterisks → Replace("*","") → "" → int.Parse("") throws
        SetupGetData([ValidDto() with { AccountNumberMaskfirst10Digits = "****************" }]);

        using VcfServices sut = BuildSut();
        bool result = await sut.ProcessFileAsync();

        Assert.False(result);
        _fileServicesMock.Verify(
            f => f.HandleFileError(
                _tempDir, "mask_bad.csv", It.IsAny<string>(), It.IsAny<string>(),
                It.Is<List<string>>(errs => errs.Count >= 1)),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // ImportFile metadata
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ProcessFileAsync_HappyPath_ImportFileHasCorrectFileNames()
    {
        WriteCsvFile("mydata.csv");
        SetupGetData([ValidDto()]);

        ImportFile? capturedFile = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ImportFile>()))
            .Callback<ImportFile>(f => capturedFile = f)
            .Returns(Task.CompletedTask);

        using VcfServices sut = BuildSut();
        await sut.ProcessFileAsync();

        Assert.NotNull(capturedFile);
        Assert.Equal("mydata.csv", capturedFile!.ImportFileName);
        Assert.StartsWith("mydata_",   capturedFile.ArchiveFileName);
        Assert.EndsWith(".csv",        capturedFile.ArchiveFileName);
    }
}

using GenericRepositories.Interfaces;
using MagellanFileServices.Contracts;
using MagellanFileServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using VCFFileImport.Contracts;
using VCFFileImport.Data;
using VCFFileImport.Models;
using VCFFileImport.Models.Database;
using VCFFileImport.Models.DTOs;

namespace VCFFileImport.Services
{
    internal class VcfServices : IVcfServices
    {
        internal readonly IGenericRepository<ImportFile,VcfContext,int> _repository;
        internal readonly IFileServices _fileServices;

        ILogger<VcfServices> _logger;
        private bool disposedValue;
        private readonly string _basePath;

        public VcfServices(IGenericRepository<ImportFile, VcfContext, int> repository, 
                            ILogger<VcfServices> logger, 
                            IConfiguration config,
                            IFileServices fileServices)
        {
            _repository = repository;
            _logger = logger;
            _basePath = config.GetValue<string>("Data:BasePath") ?? "";
            _fileServices = fileServices;

            if(String.IsNullOrWhiteSpace(_basePath))
            {
                throw new Exception("BasePath missing");
            }
        }        
        public async Task<bool> ProcessFileAsync()
        {
            bool success = true;
            try
            {
                DirectoryInfo di = new(_basePath);

                foreach(FileInfo file in di.GetFiles("*.csv"))
                {   
                    DateTime dateTimeAddedUtc = DateTime.UtcNow;
                    string timeStamp = dateTimeAddedUtc.ToString("yyyyMMddHHmmssffff");

                    ImportFile? existingFile = await _repository.FindFirstAsync(e => e.ImportFileName == file.Name, q => q.OrderBy(f => f.ImportFileName));

                    if (existingFile == null)
                    {                        
                        try
                        {
                            _logger.LogInformation($"Processing file {file.Name}");
                            ObjectResult<VcfTransactionDto> importResult = _fileServices.GetDataFromFile<VcfTransactionDto>(Path.Combine(_basePath, file.Name),
                                                                                                                            encoding: Encoding.UTF8,
                                                                                                                            rowsToSkip:0,
                                                                                                                            fixUnescapedQuotes: true);
                            List<VcfTransactionDto> transactionDtos = importResult.ObjectResults;
                            UpdateResult result = await UpdateDatabaseAsync(transactionDtos, dateTimeAddedUtc, file.Name);
                            

                            if (result.Errors.Count > 0 || importResult.Errors.Count > 0)
                            {

                                List<string> errors = [];
                                errors = result.Errors;
                                errors.AddRange(importResult.Errors);
                                _fileServices.HandleFileError(_basePath, file.Name,$"File {file.Name} had the following errors",  timeStamp, errors);
                                success = false;
                            }
                            else
                            {
                                _fileServices.HandleFileSuccess(_basePath, file.Name, timeStamp);
                            }
                        }
                        catch (Exception ex)
                        {
                            _fileServices.HandleFileError(_basePath, file.Name,$"File {file.Name} error: {ex.Message}",  timeStamp);
                            success = false;
                        }
                    }
                    else
                    {
                        _fileServices.HandleFileError(_basePath,file.Name,$"File {file.Name} already imported.",timeStamp);
                        success = false;
                    }                 
                    
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }        
        private async Task<UpdateResult> UpdateDatabaseAsync(List<VcfTransactionDto> transactions, DateTime dateTimeAddedUtc, string fileName)
        {
            try
            {
                int rowCount = 0;
                List<string> errors = [];
                List<VcfTransaction> vcfTransactions = [];

                foreach(VcfTransactionDto dto in transactions)
                {                    
                    try
                    {
                        VcfTransaction vcfTransaction = MapTransaction(dto);
                        vcfTransactions.Add(vcfTransaction);                        
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Row {rowCount} failed: error: {ex.Message}");
                        errors.Add($"Row {rowCount} failed: error: {ex.Message}");
                    }      
                    rowCount++;
                }

                ImportFile importFile = new()
                {
                    ImportFileName = fileName,
                    ArchiveFileName = fileName.Replace(".csv", $"_{dateTimeAddedUtc.ToString("yyyyMMddHHmmssffff")}.csv"),
                    DateTimeAddedUtc = dateTimeAddedUtc,
                    ImportFileVcfTransactions = vcfTransactions.Select(s => new ImportFileVcfTransaction() {VcfTransaction = s }).ToList()
                };
                await _repository.AddAsync(importFile);
                int recordsUpdated = await _repository.SaveChangesAsync();
                return new UpdateResult(recordsUpdated, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
        private VcfTransaction MapTransaction(VcfTransactionDto dto)
        {
            try
            {
                int AccountNumberLastSix = int.Parse(dto.AccountNumberMaskfirst10Digits.Replace("*", ""));

                VcfTransaction transaction = new()
                { 
                    AccountNumberMaskfirst10Digits = dto.AccountNumberMaskfirst10Digits,
                    AccountNumberLastSix = AccountNumberLastSix,
                    AccountOpenDate = (DateTime)GetDateFromString(dto.AccountOpenDate),
                    AccountCloseDate = GetDateFromString(dto.AccountCloseDate),
                    BillingAmount = Decimal.Parse(dto.BillingAmount),
                    TransactionDate = (DateTime)GetDateFromString(dto.TransactionDate),
                    PostingDate = (DateTime)GetDateFromString(dto.PostingDate),
                    TransactionTypeCode = dto.TransactionTypeCode,
                    CommodityCode = String.IsNullOrWhiteSpace(dto.CommodityCode) ? null : dto.CommodityCode,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    EmployeeID = dto.EmployeeID,
                    TransactionReferenceNumber = dto.TransactionReferenceNumber,
                    LastCreditLimitChangeDate = GetDateFromString(dto.LastCreditLimitChangeDate),
                    StatusCode = int.Parse(dto.StatusCode),
                    StatusDate = (DateTime)GetDateFromString(dto.StatusDate),
                    SupplierName = String.IsNullOrWhiteSpace(dto.SupplierName) ? null : dto.SupplierName,
                    MerchantCategoryCode = int.Parse(dto.MerchantCategoryCode),
                    PurchaseIdentification = String.IsNullOrWhiteSpace(dto.PurchaseIdentification) ? null : dto.PurchaseIdentification,
                    CardholderTransactionApproval = dto.CardholderTransactionApproval == "Y",
                    SupplierCity = String.IsNullOrWhiteSpace(dto.SupplierCity) ? null : dto.SupplierCity,
                    SupplierState = String.IsNullOrWhiteSpace(dto.SupplierState) ? null : dto.SupplierState,
                    SupplierZipCode = String.IsNullOrWhiteSpace(dto.SupplierZipCode) ? null : dto.SupplierZipCode,
                    CompanyName = String.IsNullOrWhiteSpace(dto.CompanyName) ? null : dto.CompanyName
                };
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
        private DateTime? GetDateFromString(string dateString)
        {
            try
            {
                string datePattern = "MMddyyyy";
                if(DateTime.TryParseExact(dateString,datePattern, null, DateTimeStyles.None, out DateTime date))
                {  
                    return date; 
                }
                return null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }
                _repository.Dispose();
                disposedValue = true;
            }
        }
        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VcfServices()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

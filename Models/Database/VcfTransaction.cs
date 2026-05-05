using System.Diagnostics.CodeAnalysis;

namespace VCFFileImport.Models.Database
{
    internal class VcfTransaction
    {
        [SetsRequiredMembers]
        public VcfTransaction()
        {
            AccountNumberMaskfirst10Digits = "";
            TransactionTypeCode = "";
            FirstName = "";
            LastName = "";
            EmployeeID = "";
            TransactionReferenceNumber = "";
        }

        public int Id { get; set; }
        public required string AccountNumberMaskfirst10Digits { get; set; }
        public int AccountNumberLastSix { get; set; }
        public DateTime AccountOpenDate { get; set; }
        public DateTime? AccountCloseDate { get; set; }
        public decimal BillingAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime PostingDate { get; set; }
        public required string TransactionTypeCode { get; set; }
        public string? CommodityCode { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string EmployeeID { get; set; }
        public required string TransactionReferenceNumber { get; set; }
        public DateTime? LastCreditLimitChangeDate { get; set; }
        public int StatusCode { get; set; }
        public DateTime StatusDate { get; set; }
        public string? SupplierName { get; set; }
        public int MerchantCategoryCode { get; set; }
        public string? PurchaseIdentification { get; set; }
        public bool CardholderTransactionApproval { get; set; }
        public string? SupplierCity { get; set; }
        public string? SupplierState { get; set; }
        public string? SupplierZipCode { get; set; }
        public string? CompanyName { get; set; }
    }
}

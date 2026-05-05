using CsvHelper.Configuration.Attributes;

namespace VCFFileImport.Models.DTOs
{
    
    internal record VcfTransactionDto
    {
        [Name("Account Number_ Mask first 10 Digits")]
        public required string AccountNumberMaskfirst10Digits { get; set; }
        [Name("Account Open Date")]
        public required string AccountOpenDate { get; set; }
        [Name("Account Close Date")]
        public required string AccountCloseDate { get; set; }
        [Name("Billing Amount")]
        public required string BillingAmount { get; set; }
        [Name("Transaction Date")]
        public required string TransactionDate { get; set; }
        [Name("Posting Date")]
        public required string PostingDate { get; set; }
        [Name("Transaction Type Code")]
        public required string TransactionTypeCode { get; set; }
        [Name("Commodity Code")]
        public required string CommodityCode { get; set; }
        [Name("First Name")]
        public required string FirstName { get; set; }
        [Name("Last Name")]
        public required string LastName { get; set; }
        [Name("Employee ID")]
        public required string EmployeeID { get; set; }
        [Name("Transaction Reference Number")]
        public required string TransactionReferenceNumber { get; set; }
        [Name("Last Credit Limit Change Date")]
        public required string LastCreditLimitChangeDate { get; set; }
        [Name("Status Code")]
        public required string StatusCode { get; set; }
        [Name("Status Date")]
        public required string StatusDate { get; set; }
        [Name("Supplier Name")]
        public required string SupplierName { get; set; }
        [Name("Merchant Category Code")]
        public required string MerchantCategoryCode { get; set; }
        [Name("Purchase Identification")]
        public required string PurchaseIdentification { get; set; }
        [Name("Cardholder Transaction Approval")]
        public required string CardholderTransactionApproval { get; set; }
        [Name("Supplier City")]
        public required string SupplierCity { get; set; }
        [Name("Supplier State")]
        public required string SupplierState { get; set; }
        [Name("Supplier Zip Code")]
        public required string SupplierZipCode { get; set; }
        [Name("Company Name")]
        public required string CompanyName { get; set; }

    }
}

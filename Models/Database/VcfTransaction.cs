using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace VCFFileImport.Models.Database
{
    [Table("VcfTransactions",Schema ="vcf")]
    [Index(nameof(TransactionReferenceNumber))]
    [Index(nameof(LastName),nameof(FirstName))]
    [Index(nameof(EmployeeID))]
    [Index(nameof(TransactionDate))]
    [Index(nameof(PostingDate))]
    [Index(nameof(CompanyName))]

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
        [Key]
        [Column("VcfTransactionId")]
        public int Id { get; set; }
        [MaxLength(50)]
        public required string AccountNumberMaskfirst10Digits { get; set; }
        public int AccountNumberLastSix { get; set; }
        public DateTime AccountOpenDate { get; set; }
        public DateTime? AccountCloseDate { get; set; }
        [Precision(18,2)]
        public decimal BillingAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime PostingDate { get; set; }
        [MaxLength(10)]
        public required string TransactionTypeCode { get; set; }
        [MaxLength(10)]
        public string? CommodityCode { get; set; }
        [MaxLength(200)]
        public required string FirstName { get; set; }
        [MaxLength (200)]
        public required string LastName { get; set; }
        [MaxLength(8)]
        public required string EmployeeID { get; set; }
        [MaxLength(32)]
        public required string TransactionReferenceNumber { get; set; }
        public DateTime? LastCreditLimitChangeDate { get; set; }
        public int StatusCode { get; set; }
        public DateTime StatusDate { get; set; }
        [MaxLength(200)]
        public string? SupplierName { get; set; }
        public int MerchantCategoryCode { get; set; }
        [MaxLength(50)]
        public string? PurchaseIdentification { get; set; }
        public bool CardholderTransactionApproval { get; set; }
        [MaxLength(200)]
        public string? SupplierCity { get; set; }
        [MaxLength(10)]
        public string? SupplierState { get; set; }
        [MaxLength(16)]
        public string? SupplierZipCode { get; set; }
        [MaxLength(50)]
        public string? CompanyName { get; set; }

    }
}

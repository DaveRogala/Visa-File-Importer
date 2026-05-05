using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VCFFileImport.Models.Database;

namespace VCFFileImport.Data.Configurations
{
    internal class VcfTransactionConfiguration : IEntityTypeConfiguration<VcfTransaction>
    {
        public void Configure(EntityTypeBuilder<VcfTransaction> builder)
        {
            builder.ToTable("VcfTransactions", "vcf");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("VcfTransactionId");

            builder.Property(x => x.AccountNumberMaskfirst10Digits).IsRequired().HasMaxLength(50);
            builder.Property(x => x.TransactionTypeCode).IsRequired().HasMaxLength(10);
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.EmployeeID).IsRequired().HasMaxLength(8);
            builder.Property(x => x.TransactionReferenceNumber).IsRequired().HasMaxLength(32);
            builder.Property(x => x.CommodityCode).HasMaxLength(10);
            builder.Property(x => x.SupplierName).HasMaxLength(200);
            builder.Property(x => x.PurchaseIdentification).HasMaxLength(50);
            builder.Property(x => x.SupplierCity).HasMaxLength(200);
            builder.Property(x => x.SupplierState).HasMaxLength(10);
            builder.Property(x => x.SupplierZipCode).HasMaxLength(16);
            builder.Property(x => x.CompanyName).HasMaxLength(50);
            builder.Property(x => x.BillingAmount).HasPrecision(18, 2);

            builder.HasIndex(x => x.TransactionReferenceNumber);
            builder.HasIndex(x => new { x.LastName, x.FirstName });
            builder.HasIndex(x => x.EmployeeID);
            builder.HasIndex(x => x.TransactionDate);
            builder.HasIndex(x => x.PostingDate);
            builder.HasIndex(x => x.CompanyName);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VCFFileImport.Models.Database;

namespace VCFFileImport.Data.Configurations
{
    internal class ImportFileVcfTransactionConfiguration : IEntityTypeConfiguration<ImportFileVcfTransaction>
    {
        public void Configure(EntityTypeBuilder<ImportFileVcfTransaction> builder)
        {
            builder.ToTable("ImportFileVCFTransactions", "vcf");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("ImportFileVcfTransactionId");

            builder.HasOne(x => x.ImportFile)
                .WithMany(x => x.ImportFileVcfTransactions)
                .HasForeignKey(x => x.ImportFileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.VcfTransaction)
                .WithMany()
                .HasForeignKey(x => x.VcfTransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

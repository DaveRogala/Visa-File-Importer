using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VCFFileImport.Models.Database;

namespace VCFFileImport.Data.Configurations
{
    internal class ImportFileConfiguration : IEntityTypeConfiguration<ImportFile>
    {
        public void Configure(EntityTypeBuilder<ImportFile> builder)
        {
            builder.ToTable("ImportFiles", "vcf");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("ImportFileId");

            builder.Property(x => x.ImportFileName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ArchiveFileName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.DateTimeAddedUtc).IsRequired();

            builder.HasIndex(x => x.ImportFileName).IsUnique();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace VCFFileImport.Models.Database
{
    [Table("ImportFiles",Schema ="vcf")]
    [Index(nameof(ImportFileName))]
    internal class ImportFile
    {
        [SetsRequiredMembers]
        public ImportFile()
        {
            ImportFileName = "";
            ArchiveFileName = "";
            DateTimeAddedUtc = DateTime.UtcNow;
            ImportFileVcfTransactions = [];
        }

        [Column("ImportFileId")]
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public required string ImportFileName { get; set; }
        [MaxLength(200)]
        public required string ArchiveFileName { get; set; }
        public required DateTime DateTimeAddedUtc { get; set; }
        public virtual List<ImportFileVcfTransaction> ImportFileVcfTransactions { get; set; }
    }
}

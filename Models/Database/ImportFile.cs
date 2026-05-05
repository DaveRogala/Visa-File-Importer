using System.Diagnostics.CodeAnalysis;

namespace VCFFileImport.Models.Database
{
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

        public int Id { get; set; }
        public required string ImportFileName { get; set; }
        public required string ArchiveFileName { get; set; }
        public required DateTime DateTimeAddedUtc { get; set; }
        public virtual List<ImportFileVcfTransaction> ImportFileVcfTransactions { get; set; }
    }
}

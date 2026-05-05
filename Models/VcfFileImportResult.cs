using VCFFileImport.Models.DTOs;

namespace VCFFileImport.Models
{
    internal class VcfFileImportResult
    {
        public List<VcfTransactionDto> VcfTransactionDtos { get; set; } = [];
        public List<string> BadRecords { get; set; } = [];
    }
}

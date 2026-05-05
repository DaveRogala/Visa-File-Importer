namespace VCFFileImport.Models.Database
{
    internal class ImportFileVcfTransaction
    {
        public int Id { get; set; }
        public int ImportFileId { get; set; }
        public int VcfTransactionId { get; set; }

        public virtual ImportFile ImportFile { get; set; } = null!;
        public virtual VcfTransaction VcfTransaction { get; set; } = null!;
    }
}

namespace VCFFileImport.Models
{
    internal record UpdateResult
    {
        public UpdateResult(int recordCount, List<string> errors)
        {
            RecordCount = recordCount;
            Errors = errors;
        }
        public int RecordCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}

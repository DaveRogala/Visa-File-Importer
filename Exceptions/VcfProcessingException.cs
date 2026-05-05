namespace VCFFileImport.Exceptions
{
    internal class VcfProcessingException : Exception
    {
        public VcfProcessingException(string message) : base(message) { }
        public VcfProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }
}

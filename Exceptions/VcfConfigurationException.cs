namespace VCFFileImport.Exceptions
{
    internal class VcfConfigurationException : InvalidOperationException
    {
        public VcfConfigurationException(string message) : base(message) { }
        public VcfConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}

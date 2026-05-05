namespace VCFFileImport.Contracts
{
    internal interface IVcfServices : IDisposable
    {
        Task<bool> ProcessFileAsync();
    }
}

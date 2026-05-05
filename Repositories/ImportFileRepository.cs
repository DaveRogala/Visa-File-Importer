using GenericRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VCFFileImport.Data;
using VCFFileImport.Models.Database;

namespace VCFFileImport.Repositories
{
    internal class ImportFileRepository(IDbContextFactory<VcfContext> contextFactory, ILogger<ImportFileRepository> logger)
        : GenericRepository<ImportFile,VcfContext,int>(contextFactory.CreateDbContext(), logger);
   
}

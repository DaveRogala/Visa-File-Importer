using Microsoft.EntityFrameworkCore;
using VCFFileImport.Models.Database;


namespace VCFFileImport.Data
{
    internal class VcfContext : DbContext
    {
        public VcfContext()
        {            
        }
        public VcfContext(DbContextOptions<VcfContext> options)
            :base(options) 
        {            
        }
        internal virtual DbSet<VcfTransaction> VcfTransactions { get; set; }
        internal virtual DbSet<ImportFile> ImportFiles { get; set; }
        internal virtual DbSet<ImportFileVcfTransaction> ImportFilesVcfTransactions { get; set; }
    }
}

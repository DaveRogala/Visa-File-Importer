using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace VCFFileImport.Models.Database
{
    [Table("ImportFileVCFTransactions", Schema = "vcf")]
    internal class ImportFileVcfTransaction
    {        
        [Key]
        [Column("ImportFileVcfTransactionId")]
        public int Id { get; set; }
        [ForeignKey(nameof(ImportFile))]
        public int ImportFileId { get; set; }
        [ForeignKey(nameof(VcfTransaction))]
        public int VcfTransactionId { get; set; }

        public virtual ImportFile ImportFile { get; set; }
        public virtual VcfTransaction VcfTransaction { get; set; }
    }
}

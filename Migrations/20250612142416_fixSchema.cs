using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCFFileImport.Migrations
{
    /// <inheritdoc />
    public partial class fixSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ImportFileVCFTransactions",
                newName: "ImportFileVCFTransactions",
                newSchema: "vcf");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ImportFileVCFTransactions",
                schema: "vcf",
                newName: "ImportFileVCFTransactions");
        }
    }
}

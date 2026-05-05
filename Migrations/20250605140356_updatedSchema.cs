using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCFFileImport.Migrations
{
    /// <inheritdoc />
    public partial class updatedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vcf");

            migrationBuilder.RenameTable(
                name: "VcfTransactions",
                schema: "vco",
                newName: "VcfTransactions",
                newSchema: "vcf");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vco");

            migrationBuilder.RenameTable(
                name: "VcfTransactions",
                schema: "vcf",
                newName: "VcfTransactions",
                newSchema: "vco");
        }
    }
}

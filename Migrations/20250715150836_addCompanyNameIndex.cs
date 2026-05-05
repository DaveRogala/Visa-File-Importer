using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCFFileImport.Migrations
{
    /// <inheritdoc />
    public partial class addCompanyNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_CompanyName",
                schema: "vcf",
                table: "VcfTransactions",
                column: "CompanyName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VcfTransactions_CompanyName",
                schema: "vcf",
                table: "VcfTransactions");
        }
    }
}

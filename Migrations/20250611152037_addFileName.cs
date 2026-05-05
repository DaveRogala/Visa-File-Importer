using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCFFileImport.Migrations
{
    /// <inheritdoc />
    public partial class addFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VcfTransactions_TransactionReferenceNumber",
                schema: "vcf",
                table: "VcfTransactions");

            migrationBuilder.AddColumn<string>(
                name: "SourceFileName",
                schema: "vcf",
                table: "VcfTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_SourceFileName",
                schema: "vcf",
                table: "VcfTransactions",
                column: "SourceFileName");

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_TransactionReferenceNumber",
                schema: "vcf",
                table: "VcfTransactions",
                column: "TransactionReferenceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VcfTransactions_SourceFileName",
                schema: "vcf",
                table: "VcfTransactions");

            migrationBuilder.DropIndex(
                name: "IX_VcfTransactions_TransactionReferenceNumber",
                schema: "vcf",
                table: "VcfTransactions");

            migrationBuilder.DropColumn(
                name: "SourceFileName",
                schema: "vcf",
                table: "VcfTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_TransactionReferenceNumber",
                schema: "vcf",
                table: "VcfTransactions",
                column: "TransactionReferenceNumber",
                unique: true);
        }
    }
}

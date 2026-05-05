using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCFFileImport.Migrations
{
    /// <inheritdoc />
    public partial class addImportFileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VcfTransactions_SourceFileName",
                schema: "vcf",
                table: "VcfTransactions");

            migrationBuilder.DropColumn(
                name: "DateTimeAddedUTC",
                schema: "vcf",
                table: "VcfTransactions");

            migrationBuilder.DropColumn(
                name: "SourceFileName",
                schema: "vcf",
                table: "VcfTransactions");

            migrationBuilder.CreateTable(
                name: "ImportFiles",
                schema: "vcf",
                columns: table => new
                {
                    ImportFileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArchiveFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateTimeAddedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportFiles", x => x.ImportFileId);
                });

            migrationBuilder.CreateTable(
                name: "ImportFileVCFTransactions",
                columns: table => new
                {
                    ImportFileVcfTransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportFileId = table.Column<int>(type: "int", nullable: false),
                    VcfTransactionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportFileVCFTransactions", x => x.ImportFileVcfTransactionId);
                    table.ForeignKey(
                        name: "FK_ImportFileVCFTransactions_ImportFiles_ImportFileId",
                        column: x => x.ImportFileId,
                        principalSchema: "vcf",
                        principalTable: "ImportFiles",
                        principalColumn: "ImportFileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportFileVCFTransactions_VcfTransactions_VcfTransactionId",
                        column: x => x.VcfTransactionId,
                        principalSchema: "vcf",
                        principalTable: "VcfTransactions",
                        principalColumn: "VcfTransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportFiles_ImportFileName",
                schema: "vcf",
                table: "ImportFiles",
                column: "ImportFileName");

            migrationBuilder.CreateIndex(
                name: "IX_ImportFileVCFTransactions_ImportFileId",
                table: "ImportFileVCFTransactions",
                column: "ImportFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportFileVCFTransactions_VcfTransactionId",
                table: "ImportFileVCFTransactions",
                column: "VcfTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportFileVCFTransactions");

            migrationBuilder.DropTable(
                name: "ImportFiles",
                schema: "vcf");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeAddedUTC",
                schema: "vcf",
                table: "VcfTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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
        }
    }
}

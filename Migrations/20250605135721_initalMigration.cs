using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCFFileImport.Migrations
{
    /// <inheritdoc />
    public partial class initalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vco");

            migrationBuilder.CreateTable(
                name: "VcfTransactions",
                schema: "vco",
                columns: table => new
                {
                    VcfTransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumberMaskfirst10Digits = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountNumberLastSix = table.Column<int>(type: "int", nullable: false),
                    AccountOpenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountCloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionTypeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CommodityCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmployeeID = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    TransactionReferenceNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LastCreditLimitChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    StatusDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MerchantCategoryCode = table.Column<int>(type: "int", nullable: false),
                    PurchaseIdentification = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardholderTransactionApproval = table.Column<bool>(type: "bit", nullable: false),
                    SupplierCity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierState = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SupplierZipCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VcfTransactions", x => x.VcfTransactionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_EmployeeID",
                schema: "vco",
                table: "VcfTransactions",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_LastName_FirstName",
                schema: "vco",
                table: "VcfTransactions",
                columns: new[] { "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_PostingDate",
                schema: "vco",
                table: "VcfTransactions",
                column: "PostingDate");

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_TransactionDate",
                schema: "vco",
                table: "VcfTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_VcfTransactions_TransactionReferenceNumber",
                schema: "vco",
                table: "VcfTransactions",
                column: "TransactionReferenceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VcfTransactions",
                schema: "vco");
        }
    }
}

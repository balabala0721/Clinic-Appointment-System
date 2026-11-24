using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations
{
    /// <inheritdoc />
    public partial class CreateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Medicines_MedicineId",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InvoiceItems_Type",
                table: "InvoiceItems");

            migrationBuilder.RenameColumn(
                name: "MedicineId",
                table: "Prescriptions",
                newName: "DrugId");

            migrationBuilder.RenameIndex(
                name: "IX_Prescriptions_MedicineId",
                table: "Prescriptions",
                newName: "IX_Prescriptions_DrugId");

            migrationBuilder.AddColumn<int>(
                name: "DrugId1",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", maxLength: 11, nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DrugCategory = table.Column<int>(type: "int", nullable: false),
                    MedicalUses = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    StockStatus = table.Column<int>(type: "int", nullable: false),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.DrugId);
                    table.CheckConstraint("CK_Drugs_Stock", "[StockQuantity] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "GeneratedReportHistories",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", maxLength: 11, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    GeneratedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedReportHistories", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK_GeneratedReportHistories_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DrugId1",
                table: "Prescriptions",
                column: "DrugId1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InvoiceItems_Type",
                table: "InvoiceItems",
                sql: "[ItemType] IN ('Consultation','Drug','Other')");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedReportHistories_UserID",
                table: "GeneratedReportHistories",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Drugs_DrugId",
                table: "Prescriptions",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "DrugId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Drugs_DrugId1",
                table: "Prescriptions",
                column: "DrugId1",
                principalTable: "Drugs",
                principalColumn: "DrugId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Drugs_DrugId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Drugs_DrugId1",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "GeneratedReportHistories");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DrugId1",
                table: "Prescriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InvoiceItems_Type",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "DrugId1",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "DrugId",
                table: "Prescriptions",
                newName: "MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_Prescriptions_DrugId",
                table: "Prescriptions",
                newName: "IX_Prescriptions_MedicineId");

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    MedicineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DosageForm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    GenericName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReorderLevel = table.Column<int>(type: "int", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.MedicineId);
                    table.CheckConstraint("CK_Medicines_Stock", "[StockQuantity] >= 0");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_InvoiceItems_Type",
                table: "InvoiceItems",
                sql: "[ItemType] IN ('Consultation','Medicine','Other')");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Medicines_MedicineId",
                table: "Prescriptions",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

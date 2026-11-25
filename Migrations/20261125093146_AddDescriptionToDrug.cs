
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

public partial class _20261125093146_AddDescriptionToDrug : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Remove the foreign key before changing DrugId
        migrationBuilder.DropForeignKey(
            name: "FK_Prescriptions_Drugs_DrugId",
            table: "Prescriptions");

        // 2. Remove the primary key before changing DrugID
        migrationBuilder.DropPrimaryKey(
            name: "PK_Drugs",
            table: "Drugs");

        // 3. Remove old StockStatus column
        migrationBuilder.DropColumn(
            name: "StockStatus",
            table: "Drugs");

        // 4. Change Prescription DrugId from int to string
        migrationBuilder.AlterColumn<string>(
            name: "DrugId",
            table: "Prescriptions",
            type: "nvarchar(11)",
            maxLength: 11,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        // 5. Change DrugID from int to string
        migrationBuilder.AlterColumn<string>(
            name: "DrugID",
            table: "Drugs",
            type: "nvarchar(11)",
            maxLength: 11,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldMaxLength: 11);

        // 6. Add Description column
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Drugs",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        // 7. Restore primary key
        migrationBuilder.AddPrimaryKey(
            name: "PK_Drugs",
            table: "Drugs",
            column: "DrugID");

        // 8. Restore foreign key
        migrationBuilder.AddForeignKey(
            name: "FK_Prescriptions_Drugs_DrugId",
            table: "Prescriptions",
            column: "DrugId",
            principalTable: "Drugs",
            principalColumn: "DrugID",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // 1. Remove foreign key
        migrationBuilder.DropForeignKey(
            name: "FK_Prescriptions_Drugs_DrugId",
            table: "Prescriptions");

        // 2. Remove primary key
        migrationBuilder.DropPrimaryKey(
            name: "PK_Drugs",
            table: "Drugs");

        // 3. Remove Description
        migrationBuilder.DropColumn(
            name: "Description",
            table: "Drugs");

        // 4. Restore original column types
        migrationBuilder.AlterColumn<int>(
            name: "DrugId",
            table: "Prescriptions",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(11)",
            oldMaxLength: 11);

        migrationBuilder.AlterColumn<int>(
            name: "DrugID",
            table: "Drugs",
            type: "int",
            maxLength: 11,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(11)",
            oldMaxLength: 11);

        // 5. Restore primary key
        migrationBuilder.AddPrimaryKey(
            name: "PK_Drugs",
            table: "Drugs",
            column: "DrugID");

        // 6. Restore original foreign key
        migrationBuilder.AddForeignKey(
            name: "FK_Prescriptions_Drugs_DrugId",
            table: "Prescriptions",
            column: "DrugId",
            principalTable: "Drugs",
            principalColumn: "DrugID",
            onDelete: ReferentialAction.Restrict);

        // 7. Restore old StockStatus column
        migrationBuilder.AddColumn<string>(
            name: "StockStatus",
            table: "Drugs",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}

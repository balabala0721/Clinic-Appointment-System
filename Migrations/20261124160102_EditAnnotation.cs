using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261124160102_EditAnnotation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Patients_UserId",
            table: "Patients");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Patients",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldMaxLength: 11,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Bio",
            table: "Doctors",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_UserId",
            table: "Patients",
            column: "UserId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Patients_UserId",
            table: "Patients");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Patients",
            type: "int",
            maxLength: 11,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "Bio",
            table: "Doctors",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Patients_UserId",
            table: "Patients",
            column: "UserId",
            unique: true,
            filter: "[UserId] IS NOT NULL");
    }
}

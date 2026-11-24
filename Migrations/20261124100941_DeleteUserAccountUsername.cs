using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261124100941_DeleteUserAccountUsername : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Username",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "User_Username",
            table: "Patients");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_EmergencyContactPhone",
            table: "Patients",
            column: "EmergencyContactPhone",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Patients_EmergencyContactPhone",
            table: "Patients");

        migrationBuilder.AddColumn<string>(
            name: "Username",
            table: "Users",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "User_Username",
            table: "Patients",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "");
    }
}

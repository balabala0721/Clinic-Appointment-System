using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261123113312_AddDoctorColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "MMCRegistrationNo",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "Phone",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "PhotoUrl",
            table: "Doctors");

        migrationBuilder.RenameColumn(
            name: "FullName",
            table: "Doctors",
            newName: "Qualifications");

        migrationBuilder.AlterColumn<string>(
            name: "Bio",
            table: "Doctors",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000,
            oldNullable: true);

        migrationBuilder.AddColumn<int>(
            name: "UserId1",
            table: "Doctors",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "YearsOfExperience",
            table: "Doctors",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_Doctors_UserId1",
            table: "Doctors",
            column: "UserId1");

        migrationBuilder.AddForeignKey(
            name: "FK_Doctors_Users_UserId1",
            table: "Doctors",
            column: "UserId1",
            principalTable: "Users",
            principalColumn: "UserId",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Doctors_Users_UserId1",
            table: "Doctors");

        migrationBuilder.DropIndex(
            name: "IX_Doctors_UserId1",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "UserId1",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "YearsOfExperience",
            table: "Doctors");

        migrationBuilder.RenameColumn(
            name: "Qualifications",
            table: "Doctors",
            newName: "FullName");

        migrationBuilder.AlterColumn<string>(
            name: "Bio",
            table: "Doctors",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Doctors",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "MMCRegistrationNo",
            table: "Doctors",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Phone",
            table: "Doctors",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PhotoUrl",
            table: "Doctors",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: true);
    }
}

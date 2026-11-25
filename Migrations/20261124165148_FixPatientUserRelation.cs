using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261124165148_FixPatientUserRelation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "User_AccountStatus",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_CreatedAt",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_Email",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_FullName",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_PasswordHash",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_PhoneNo",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_PhotoUrl",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_Role",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_UserId",
            table: "Patients");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "User_AccountStatus",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "User_CreatedAt",
            table: "Patients",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "SYSUTCDATETIME()");

        migrationBuilder.AddColumn<string>(
            name: "User_Email",
            table: "Patients",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "User_FullName",
            table: "Patients",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "User_PasswordHash",
            table: "Patients",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "User_PhoneNo",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "User_PhotoUrl",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "User_Role",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "User_UserId",
            table: "Patients",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }
}

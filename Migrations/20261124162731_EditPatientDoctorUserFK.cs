using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261124162731_EditPatientDoctorUserFK : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "User_UserId",
            table: "Patients",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "User_Role",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "User_PasswordHash",
            table: "Patients",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(256)",
            oldMaxLength: 256,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "User_FullName",
            table: "Patients",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "User_Email",
            table: "Patients",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "User_CreatedAt",
            table: "Patients",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "SYSUTCDATETIME()",
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true,
            oldDefaultValueSql: "SYSUTCDATETIME()");

        migrationBuilder.AlterColumn<string>(
            name: "User_AccountStatus",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "UserId1",
            table: "Doctors",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "User_UserId",
            table: "Patients",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "User_Role",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "User_PasswordHash",
            table: "Patients",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(256)",
            oldMaxLength: 256);

        migrationBuilder.AlterColumn<string>(
            name: "User_FullName",
            table: "Patients",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "User_Email",
            table: "Patients",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<DateTime>(
            name: "User_CreatedAt",
            table: "Patients",
            type: "datetime2",
            nullable: true,
            defaultValueSql: "SYSUTCDATETIME()",
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldDefaultValueSql: "SYSUTCDATETIME()");

        migrationBuilder.AlterColumn<string>(
            name: "User_AccountStatus",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<int>(
            name: "UserId1",
            table: "Doctors",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");
    }
}

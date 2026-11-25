using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261124120001_EditTableColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Patients_EmergencyContactPhone",
            table: "Patients");

        migrationBuilder.DropIndex(
            name: "IX_Patients_IdNumber",
            table: "Patients");

        migrationBuilder.AlterColumn<string>(
            name: "PhotoUrl",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNo",
            table: "Users",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<int>(
            name: "User_UserId",
            table: "Patients",
            type: "int",
            maxLength: 11,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldMaxLength: 11);

        migrationBuilder.AlterColumn<string>(
            name: "User_Role",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "User_PhotoUrl",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "User_PhoneNo",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

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

        migrationBuilder.AlterColumn<string>(
            name: "Nationality",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "IdType",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "IdNumber",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "Gender",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "EmergencyContactPhone",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "EmergencyContactName",
            table: "Patients",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<DateOnly>(
            name: "DateOfBirth",
            table: "Patients",
            type: "date",
            nullable: true,
            oldClrType: typeof(DateOnly),
            oldType: "date");

        migrationBuilder.AlterColumn<int>(
            name: "UserId1",
            table: "Doctors",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_EmergencyContactPhone",
            table: "Patients",
            column: "EmergencyContactPhone",
            unique: true,
            filter: "[EmergencyContactPhone] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_IdNumber",
            table: "Patients",
            column: "IdNumber",
            unique: true,
            filter: "[IdNumber] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Patients_EmergencyContactPhone",
            table: "Patients");

        migrationBuilder.DropIndex(
            name: "IX_Patients_IdNumber",
            table: "Patients");

        migrationBuilder.AlterColumn<string>(
            name: "PhotoUrl",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNo",
            table: "Users",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "User_UserId",
            table: "Patients",
            type: "int",
            maxLength: 11,
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldMaxLength: 11,
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
            name: "User_PhotoUrl",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "User_PhoneNo",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
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

        migrationBuilder.AlterColumn<string>(
            name: "Nationality",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "IdType",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "IdNumber",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Gender",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "EmergencyContactPhone",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "EmergencyContactName",
            table: "Patients",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<DateOnly>(
            name: "DateOfBirth",
            table: "Patients",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(1, 1, 1),
            oldClrType: typeof(DateOnly),
            oldType: "date",
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

        migrationBuilder.CreateIndex(
            name: "IX_Patients_EmergencyContactPhone",
            table: "Patients",
            column: "EmergencyContactPhone",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Patients_IdNumber",
            table: "Patients",
            column: "IdNumber",
            unique: true);
    }
}

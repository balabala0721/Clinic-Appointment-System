using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261123104500_AddPatientColumnsAndEditSomeColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Patients_ICNumber",
            table: "Patients");

        migrationBuilder.DropIndex(
            name: "IX_Patients_UserId",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "Allergies",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "ICNumber",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "Phone",
            table: "Patients");

        migrationBuilder.RenameColumn(
            name: "FullName",
            table: "Patients",
            newName: "User_FullName");

        migrationBuilder.RenameColumn(
            name: "CreatedAt",
            table: "Patients",
            newName: "User_CreatedAt");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Patients",
            type: "int",
            maxLength: 11,
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.Sql("ALTER TABLE [Patients] DROP CONSTRAINT [CK_Patients_Gender];");

        migrationBuilder.AlterColumn<string>(
            name: "Gender",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10,
            oldNullable: true);

        migrationBuilder.Sql(
        "ALTER TABLE [Patients] ADD CONSTRAINT [CK_Patients_Gender] " +
        "CHECK ([Gender] IN ('Male','Female','Other'));");

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
            oldType: "nvarchar(150)",
            oldMaxLength: 150,
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

        migrationBuilder.AlterColumn<string>(
            name: "Address",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(300)",
            oldMaxLength: 300,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "User_FullName",
            table: "Patients",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(150)",
            oldMaxLength: 150);

        migrationBuilder.AddColumn<string>(
            name: "IdNumber",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "IdType",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "User_AccountStatus",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "User_Email",
            table: "Patients",
            type: "nvarchar(50)",
            maxLength: 50,
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
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "User_PhotoUrl",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

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
            maxLength: 11,
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "User_Username",
            table: "Patients",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_IdNumber",
            table: "Patients",
            column: "IdNumber",
            unique: true);

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
            name: "IX_Patients_IdNumber",
            table: "Patients");

        migrationBuilder.DropIndex(
            name: "IX_Patients_UserId",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "IdNumber",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "IdType",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_AccountStatus",
            table: "Patients");

        migrationBuilder.DropColumn(
            name: "User_Email",
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

        migrationBuilder.DropColumn(
            name: "User_Username",
            table: "Patients");

        migrationBuilder.RenameColumn(
            name: "User_FullName",
            table: "Patients",
            newName: "FullName");

        migrationBuilder.RenameColumn(
            name: "User_CreatedAt",
            table: "Patients",
            newName: "CreatedAt");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Patients",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldMaxLength: 11);

        migrationBuilder.AlterColumn<string>(
            name: "Gender",
            table: "Patients",
            type: "nvarchar(10)",
            maxLength: 10,
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
            type: "nvarchar(150)",
            maxLength: 150,
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

        migrationBuilder.AlterColumn<string>(
            name: "Address",
            table: "Patients",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "FullName",
            table: "Patients",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AddColumn<string>(
            name: "Allergies",
            table: "Patients",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ICNumber",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Patients",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "Phone",
            table: "Patients",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Patients_ICNumber",
            table: "Patients",
            column: "ICNumber",
            unique: true,
            filter: "[ICNumber] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Patients_UserId",
            table: "Patients",
            column: "UserId",
            unique: true,
            filter: "[UserId] IS NOT NULL");
    }
}

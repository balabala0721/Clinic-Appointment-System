using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261123152309_AddPatientNationality : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Nationality",
            table: "Patients",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Nationality",
            table: "Patients");
    }
}

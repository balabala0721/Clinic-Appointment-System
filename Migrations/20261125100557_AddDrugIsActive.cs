using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations;

/// <inheritdoc />
public partial class _20261125100557_AddDrugIsActive : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Drugs",
            type: "bit",
            nullable: false,
            defaultValue: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Drugs");
    }
}

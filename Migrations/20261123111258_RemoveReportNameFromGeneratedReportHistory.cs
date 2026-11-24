using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReportNameFromGeneratedReportHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportName",
                table: "GeneratedReportHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportName",
                table: "GeneratedReportHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}

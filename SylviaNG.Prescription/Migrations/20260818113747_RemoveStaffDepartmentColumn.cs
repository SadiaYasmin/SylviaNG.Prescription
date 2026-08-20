using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SylviaNG.Prescription.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaffDepartmentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Staff");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Staff",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}

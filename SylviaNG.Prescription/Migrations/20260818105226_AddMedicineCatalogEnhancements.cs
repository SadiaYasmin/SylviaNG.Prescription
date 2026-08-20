using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SylviaNG.Prescription.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicineCatalogEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DgdaRegistered",
                table: "Medicines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "Medicines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Medicines",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_DgdaRegistered",
                table: "Medicines",
                column: "DgdaRegistered");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Medicines_DgdaRegistered",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "DgdaRegistered",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Medicines");
        }
    }
}

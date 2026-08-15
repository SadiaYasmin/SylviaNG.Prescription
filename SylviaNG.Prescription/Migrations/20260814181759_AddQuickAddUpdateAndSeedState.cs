using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SylviaNG.Prescription.Migrations
{
    /// <inheritdoc />
    public partial class AddQuickAddUpdateAndSeedState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorQuickAddSeedStates",
                columns: table => new
                {
                    DoctorId = table.Column<long>(type: "bigint", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SeededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorQuickAddSeedStates", x => new { x.DoctorId, x.SectionType });
                    table.ForeignKey(
                        name: "FK_DoctorQuickAddSeedStates_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorQuickAddSeedStates");
        }
    }
}

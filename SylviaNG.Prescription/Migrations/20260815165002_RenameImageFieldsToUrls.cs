using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SylviaNG.Prescription.Migrations
{
    /// <inheritdoc />
    public partial class RenameImageFieldsToUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoBase64",
                table: "HospitalSettings");

            migrationBuilder.DropColumn(
                name: "SealBase64",
                table: "HospitalSettings");

            migrationBuilder.DropColumn(
                name: "PhotoBase64",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SignatureBase64",
                table: "Doctors");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "HospitalSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SealUrl",
                table: "HospitalSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Doctors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureUrl",
                table: "Doctors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "HospitalSettings");

            migrationBuilder.DropColumn(
                name: "SealUrl",
                table: "HospitalSettings");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SignatureUrl",
                table: "Doctors");

            migrationBuilder.AddColumn<string>(
                name: "LogoBase64",
                table: "HospitalSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SealBase64",
                table: "HospitalSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoBase64",
                table: "Doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureBase64",
                table: "Doctors",
                type: "text",
                nullable: true);
        }
    }
}

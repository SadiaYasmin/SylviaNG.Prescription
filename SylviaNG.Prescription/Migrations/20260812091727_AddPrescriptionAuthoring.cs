using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SylviaNG.Prescription.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionAuthoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Doctors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PreferredTemplateId",
                table: "Doctors",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureBase64",
                table: "Doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RegisteredByStaffId",
                table: "Consultations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    MedicineId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GenericName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Strength = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DosageForm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.MedicineId);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConsultationId = table.Column<long>(type: "bigint", nullable: false),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    DoctorId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ChiefComplaintsJson = table.Column<string>(type: "text", nullable: false),
                    HistoryJson = table.Column<string>(type: "text", nullable: false),
                    DiagnosesJson = table.Column<string>(type: "text", nullable: false),
                    InvestigationsJson = table.Column<string>(type: "text", nullable: false),
                    MedicinesJson = table.Column<string>(type: "text", nullable: false),
                    AdviceJson = table.Column<string>(type: "text", nullable: false),
                    Bp = table.Column<string>(type: "text", nullable: true),
                    Pulse = table.Column<string>(type: "text", nullable: true),
                    Temperature = table.Column<string>(type: "text", nullable: true),
                    RespiratoryRate = table.Column<string>(type: "text", nullable: true),
                    Spo2 = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<string>(type: "text", nullable: true),
                    Height = table.Column<string>(type: "text", nullable: true),
                    BloodSugar = table.Column<string>(type: "text", nullable: true),
                    PainScore = table.Column<string>(type: "text", nullable: true),
                    HeartRate = table.Column<string>(type: "text", nullable: true),
                    FollowUp = table.Column<string>(type: "text", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.PrescriptionId);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "ConsultationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_PrescriptionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "PrescriptionTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuickAddPresets",
                columns: table => new
                {
                    QuickAddPresetId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<long>(type: "bigint", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickAddPresets", x => x.QuickAddPresetId);
                    table.ForeignKey(
                        name: "FK_QuickAddPresets_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_PreferredTemplateId",
                table: "Doctors",
                column: "PreferredTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_Active",
                table: "Medicines",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_BrandName",
                table: "Medicines",
                column: "BrandName");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_GenericName",
                table: "Medicines",
                column: "GenericName");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ConsultationId",
                table: "Prescriptions",
                column: "ConsultationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DisplayCode",
                table: "Prescriptions",
                column: "DisplayCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Status",
                table: "Prescriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_TemplateId",
                table: "Prescriptions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QuickAddPresets_DoctorId_SectionType",
                table: "QuickAddPresets",
                columns: new[] { "DoctorId", "SectionType" });

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_PrescriptionTemplates_PreferredTemplateId",
                table: "Doctors",
                column: "PreferredTemplateId",
                principalTable: "PrescriptionTemplates",
                principalColumn: "TemplateId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_PrescriptionTemplates_PreferredTemplateId",
                table: "Doctors");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "QuickAddPresets");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_PreferredTemplateId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PreferredTemplateId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SignatureBase64",
                table: "Doctors");

            migrationBuilder.AlterColumn<long>(
                name: "RegisteredByStaffId",
                table: "Consultations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}

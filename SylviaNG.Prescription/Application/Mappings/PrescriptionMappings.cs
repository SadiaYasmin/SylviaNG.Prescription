using System.Text.Json;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    /// <summary>
    /// Get/Set pairs for PrescriptionRecord's JSON list-section columns, same portable-
    /// serialized-column pattern as TemplateMappings' GetConfig/SetConfig for PrescriptionTemplate.
    /// </summary>
    public static class PrescriptionMappings
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<string> GetChiefComplaints(this PrescriptionRecord prescription) => Deserialize<string>(prescription.ChiefComplaintsJson);
        public static void SetChiefComplaints(this PrescriptionRecord prescription, List<string> value) => prescription.ChiefComplaintsJson = Serialize(value);

        public static List<string> GetHistory(this PrescriptionRecord prescription) => Deserialize<string>(prescription.HistoryJson);
        public static void SetHistory(this PrescriptionRecord prescription, List<string> value) => prescription.HistoryJson = Serialize(value);

        public static List<string> GetInvestigations(this PrescriptionRecord prescription) => Deserialize<string>(prescription.InvestigationsJson);
        public static void SetInvestigations(this PrescriptionRecord prescription, List<string> value) => prescription.InvestigationsJson = Serialize(value);

        public static List<string> GetAdvice(this PrescriptionRecord prescription) => Deserialize<string>(prescription.AdviceJson);
        public static void SetAdvice(this PrescriptionRecord prescription, List<string> value) => prescription.AdviceJson = Serialize(value);

        public static List<DiagnosisItem> GetDiagnoses(this PrescriptionRecord prescription) => Deserialize<DiagnosisItem>(prescription.DiagnosesJson);
        public static void SetDiagnoses(this PrescriptionRecord prescription, List<DiagnosisItem> value) => prescription.DiagnosesJson = Serialize(value);

        public static List<MedicineItem> GetMedicines(this PrescriptionRecord prescription) => Deserialize<MedicineItem>(prescription.MedicinesJson);
        public static void SetMedicines(this PrescriptionRecord prescription, List<MedicineItem> value) => prescription.MedicinesJson = Serialize(value);

        private static List<T> Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();
            return JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? new List<T>();
        }

        private static string Serialize<T>(List<T> value) => JsonSerializer.Serialize(value ?? new List<T>(), SerializerOptions);

        /// <summary>Reads all six list-section columns plus the discrete vitals columns into one PrescriptionContent.</summary>
        public static PrescriptionContent GetContent(this PrescriptionRecord prescription) => new()
        {
            ChiefComplaints = prescription.GetChiefComplaints(),
            History = prescription.GetHistory(),
            Examination = new ExaminationDto
            {
                Bp = prescription.Bp,
                Pulse = prescription.Pulse,
                Temperature = prescription.Temperature,
                RespiratoryRate = prescription.RespiratoryRate,
                Spo2 = prescription.Spo2,
                Weight = prescription.Weight,
                Height = prescription.Height,
                BloodSugar = prescription.BloodSugar,
                PainScore = prescription.PainScore,
                HeartRate = prescription.HeartRate
            },
            Diagnoses = prescription.GetDiagnoses(),
            Investigations = prescription.GetInvestigations(),
            Medicines = prescription.GetMedicines(),
            Advice = prescription.GetAdvice(),
            FollowUp = prescription.FollowUp
        };

        /// <summary>Writes a full PrescriptionContent back into the six JSON columns plus discrete vitals columns.</summary>
        public static void SetContent(this PrescriptionRecord prescription, PrescriptionContent content)
        {
            prescription.SetChiefComplaints(content.ChiefComplaints ?? new List<string>());
            prescription.SetHistory(content.History ?? new List<string>());
            prescription.SetDiagnoses(content.Diagnoses ?? new List<DiagnosisItem>());
            prescription.SetInvestigations(content.Investigations ?? new List<string>());
            prescription.SetMedicines(content.Medicines ?? new List<MedicineItem>());
            prescription.SetAdvice(content.Advice ?? new List<string>());

            var exam = content.Examination ?? new ExaminationDto();
            prescription.Bp = exam.Bp;
            prescription.Pulse = exam.Pulse;
            prescription.Temperature = exam.Temperature;
            prescription.RespiratoryRate = exam.RespiratoryRate;
            prescription.Spo2 = exam.Spo2;
            prescription.Weight = exam.Weight;
            prescription.Height = exam.Height;
            prescription.BloodSugar = exam.BloodSugar;
            prescription.PainScore = exam.PainScore;
            prescription.HeartRate = exam.HeartRate;

            prescription.FollowUp = content.FollowUp;
        }

        public static PatientSnapshotResponse ToSnapshot(this Patient patient) => new()
        {
            PatientId = patient.PatientId,
            Name = patient.Name,
            Phone = patient.Phone,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.Age,
            Gender = patient.Gender,
            BloodGroup = patient.BloodGroup,
            AllergyPresetId = patient.AllergyPresetId,
            AllergyOtherText = patient.AllergyOtherText,
            SavedHistory = patient.SavedHistory
        };

        public static DoctorSnapshotResponse ToSnapshot(this Doctor doctor) => new()
        {
            DoctorId = doctor.DoctorId,
            FullName = doctor.FullName,
            Qualification = doctor.Qualification,
            Department = doctor.Department,
            LicenseNumber = doctor.LicenseNumber,
            SignatureBase64 = doctor.SignatureBase64,
            PreferredTemplateId = doctor.PreferredTemplateId
        };

        public static HospitalSettingsSnapshotResponse ToSnapshot(this HospitalSettings settings) => new()
        {
            Name = settings.Name,
            LogoBase64 = settings.LogoBase64,
            Address = settings.Address,
            Phone = settings.Phone,
            EmergencyNumber = settings.EmergencyNumber,
            Website = settings.Website,
            Slogan = settings.Slogan,
            SloganBn = settings.SloganBn,
            LicenseNumber = settings.LicenseNumber,
            SealBase64 = settings.SealBase64
        };

        /// <summary>
        /// The full render/edit payload — see PrescriptionDocumentResponse's doc comment for why
        /// this bundles template config + hospital branding inline.
        /// </summary>
        public static PrescriptionDocumentResponse ToDocumentResponse(
            this PrescriptionRecord prescription,
            Patient patient,
            Doctor doctor,
            PrescriptionTemplate template,
            HospitalSettings hospitalSettings)
        {
            return new PrescriptionDocumentResponse
            {
                PrescriptionId = prescription.PrescriptionId,
                DisplayCode = prescription.DisplayCode,
                ConsultationId = prescription.ConsultationId,
                Status = prescription.Status,
                Language = prescription.Language,
                SavedAt = prescription.SavedAt,
                FinalizedAt = prescription.FinalizedAt,
                Content = prescription.GetContent(),
                Patient = patient.ToSnapshot(),
                Doctor = doctor.ToSnapshot(),
                TemplateId = template.TemplateId,
                TemplateType = template.Type,
                TemplateConfig = template.GetConfig(),
                HospitalSettings = hospitalSettings.ToSnapshot()
            };
        }

        public static PrescriptionListItemResponse ToListItemResponse(this PrescriptionRecord prescription, string patientName, string doctorName)
        {
            return new PrescriptionListItemResponse
            {
                PrescriptionId = prescription.PrescriptionId,
                DisplayCode = prescription.DisplayCode,
                PatientId = prescription.PatientId,
                PatientName = patientName,
                DoctorId = prescription.DoctorId,
                DoctorName = doctorName,
                Status = prescription.Status,
                SavedAt = prescription.SavedAt,
                FinalizedAt = prescription.FinalizedAt,
                DiagnosisCount = prescription.GetDiagnoses().Count,
                MedicineCount = prescription.GetMedicines().Count,
                InvestigationCount = prescription.GetInvestigations().Count
            };
        }
    }
}

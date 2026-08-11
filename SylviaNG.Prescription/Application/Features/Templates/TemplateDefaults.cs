using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Templates;

/// <summary>
/// Builds a sensible default <see cref="TemplateConfig"/> per (Type, Language) combination for
/// <c>CreateTemplateHandler</c> and <c>TemplateEngineSeeder</c> — the client does not supply
/// config on create (US-046), only on update (US-047-049).
/// </summary>
public static class TemplateDefaults
{
    public static TemplateConfig BuildDefaultConfig(TemplateTypeEnum type, TemplateLanguageEnum language)
    {
        var config = type switch
        {
            TemplateTypeEnum.Classic => ClassicDefaults(),
            TemplateTypeEnum.Corporate => CorporateDefaults(),
            TemplateTypeEnum.Government => GovernmentDefaults(),
            _ => ClassicDefaults()
        };

        config.Footer.QrMessage = "Scan to verify";
        config.Footer.QrMessageBn = "যাচাই করতে স্ক্যান করুন";
        config.Labels = GetDefaultLabels(language);

        return config;
    }

    private static TemplateConfig ClassicDefaults() => new()
    {
        // White header with a teal accent underline (rendered from AccentColor) — the doctor
        // name / hospital name sit on white, not a filled banner. Corporate is the banner style.
        Header = new HeaderConfig { BgColor = null, Height = 110, LogoSize = 64, NameFont = "heading", BorderStyle = "solid" },
        Footer = new FooterConfig { BgColor = "#F0FDFA", Height = 70, BorderStyle = "solid" },
        Style = new StyleConfig { SectionSpacing = 16, BorderRadius = 8, BorderStyle = "solid", AccentColor = "#0F766E", FontFamily = "body", FontSize = 14, TableStyle = "striped", WatermarkText = null },
        Visibility = new VisibilityConfig { Logo = true, Slogan = true, Footer = true, Watermark = false },
        Print = new PrintConfig { PageSize = "A4", Orientation = "portrait", MarginMm = 12 }
    };

    private static TemplateConfig CorporateDefaults() => new()
    {
        // Full banner style, bigger header — per US-047's "corporate" description.
        Header = new HeaderConfig { BgColor = "#1E3A8A", Height = 150, LogoSize = 84, NameFont = "heading", BorderStyle = "none" },
        Footer = new FooterConfig { BgColor = "#EFF6FF", Height = 90, BorderStyle = "none" },
        Style = new StyleConfig { SectionSpacing = 20, BorderRadius = 4, BorderStyle = "none", AccentColor = "#1E3A8A", FontFamily = "heading", FontSize = 15, TableStyle = "bordered", WatermarkText = null },
        Visibility = new VisibilityConfig { Logo = true, Slogan = true, Footer = true, Watermark = false },
        Print = new PrintConfig { PageSize = "A4", Orientation = "portrait", MarginMm = 10 }
    };

    private static TemplateConfig GovernmentDefaults() => new()
    {
        // Government templates are structurally monochrome at render time (US-047) — BgColor/
        // AccentColor are left null/neutral here since rendering enforces monochrome, not the
        // backend; the backend just stores sensible neutral defaults for schema completeness.
        Header = new HeaderConfig { BgColor = null, Height = 100, LogoSize = 72, NameFont = "heading", BorderStyle = "solid" },
        Footer = new FooterConfig { BgColor = null, Height = 70, BorderStyle = "solid" },
        Style = new StyleConfig { SectionSpacing = 14, BorderRadius = 0, BorderStyle = "solid", AccentColor = null, FontFamily = "body", FontSize = 14, TableStyle = "bordered", WatermarkText = null },
        Visibility = new VisibilityConfig { Logo = true, Slogan = false, Footer = true, Watermark = false },
        Print = new PrintConfig { PageSize = "A4", Orientation = "portrait", MarginMm = 15 }
    };

    public static Dictionary<string, string> GetDefaultLabels(TemplateLanguageEnum language) =>
        language == TemplateLanguageEnum.Bn ? BanglaLabels() : EnglishLabels();

    private static Dictionary<string, string> EnglishLabels() => new()
    {
        ["patientName"] = "Name",
        ["patientAge"] = "Age",
        ["patientSex"] = "Sex",
        ["patientPhone"] = "Phone",
        ["patientBloodGroup"] = "Blood Group",
        ["patientAllergies"] = "Allergies",
        ["patientDate"] = "Date",
        ["patientRxNo"] = "Rx No",
        ["chiefComplaint"] = "C/C",
        ["history"] = "H/O",
        ["examination"] = "O/E",
        ["diagnosis"] = "Dx",
        ["investigation"] = "Inv",
        ["prescriptionHeading"] = "Rx",
        ["advice"] = "Advice",
        ["followUp"] = "Follow-Up",
        ["doctorSignature"] = "Doctor's Signature",
        ["chiefComplaintPlaceholder"] = "Fever for 3 days",
        ["advicePlaceholder"] = "Write advice here...",
        ["followUpPlaceholder"] = "e.g. Review after 7 days",
        ["vitalBP"] = "BP",
        ["vitalPulse"] = "Pulse",
        ["vitalTempEditable"] = "Temp (°F)",
        ["vitalTempReadonly"] = "Temp",
        ["vitalWeightEditable"] = "Weight (kg)",
        ["vitalWeightReadonly"] = "Weight",
        ["vitalHeightEditable"] = "Height (cm)",
        ["vitalHeightReadonly"] = "Height",
        ["vitalBMI"] = "BMI",
        ["vitalRespRate"] = "Resp. Rate",
        ["vitalSpO2Editable"] = "SpO₂ (%)",
        ["vitalSpO2Readonly"] = "SpO₂",
        ["vitalBloodSugar"] = "Blood Sugar",
        ["vitalPainScore"] = "Pain Score",
        ["vitalHeartRate"] = "Heart Rate",
        ["medDosage"] = "Dosage",
        ["medFrequency"] = "Frequency",
        ["medDuration"] = "Duration",
        ["medInstructions"] = "Instructions",
        ["medEmptyEditable"] = "No medicines added yet.",
        ["medEmptyReadonly"] = "No medicines prescribed.",
        ["medSearchPlaceholder"] = "Search medicine",
        ["medStrengthPlaceholder"] = "Strength",
        ["medDosagePlaceholder"] = "e.g. 1 Tablet",
        ["medFrequencyPlaceholder"] = "e.g. Twice Daily (BD)",
        ["medDurationPlaceholder"] = "e.g. 7 Days",
        ["medInstructionsPlaceholder"] = "e.g. After meals",
        ["regNo"] = "Reg No",
        ["bmdcRegNo"] = "BM&DC Reg. No",
        ["callPrefix"] = "Call",
        ["emergency"] = "Emergency",
        ["generatedBy"] = "Generated by PrescriptionMS",
        ["qrScanToVerify"] = "Scan to verify"
    };

    private static Dictionary<string, string> BanglaLabels() => new()
    {
        ["patientName"] = "নাম",
        ["patientAge"] = "বয়স",
        ["patientSex"] = "লিঙ্গ",
        ["patientPhone"] = "ফোন",
        ["patientBloodGroup"] = "রক্তের গ্রুপ",
        ["patientAllergies"] = "এলার্জি",
        ["patientDate"] = "তারিখ",
        ["patientRxNo"] = "Rx নং",
        ["chiefComplaint"] = "C/C",
        ["history"] = "H/O",
        ["examination"] = "O/E",
        ["diagnosis"] = "Dx",
        ["investigation"] = "Inv",
        ["prescriptionHeading"] = "Rx",
        ["advice"] = "পরামর্শ",
        ["followUp"] = "ফলো-আপ",
        ["doctorSignature"] = "ডাক্তারের স্বাক্ষর",
        ["chiefComplaintPlaceholder"] = "৩ দিন ধরে জ্বর",
        ["advicePlaceholder"] = "এখানে পরামর্শ লিখুন...",
        ["followUpPlaceholder"] = "যেমন: ৭ দিন পর আবার দেখান",
        ["vitalBP"] = "রক্তচাপ",
        ["vitalPulse"] = "নাড়ি",
        ["vitalTempEditable"] = "তাপমাত্রা (°F)",
        ["vitalTempReadonly"] = "তাপমাত্রা",
        ["vitalWeightEditable"] = "ওজন (কেজি)",
        ["vitalWeightReadonly"] = "ওজন",
        ["vitalHeightEditable"] = "উচ্চতা (সেমি)",
        ["vitalHeightReadonly"] = "উচ্চতা",
        ["vitalBMI"] = "বিএমআই",
        ["vitalRespRate"] = "শ্বাসের হার",
        ["vitalSpO2Editable"] = "SpO₂ (%)",
        ["vitalSpO2Readonly"] = "SpO₂",
        ["vitalBloodSugar"] = "রক্তে শর্করা",
        ["vitalPainScore"] = "ব্যথার মাত্রা",
        ["vitalHeartRate"] = "হৃদস্পন্দন",
        ["medDosage"] = "মাত্রা",
        ["medFrequency"] = "সেবনবিধি",
        ["medDuration"] = "মেয়াদ",
        ["medInstructions"] = "নির্দেশনা",
        ["medEmptyEditable"] = "এখনো কোনো ওষুধ যোগ করা হয়নি।",
        ["medEmptyReadonly"] = "কোনো ওষুধ নির্ধারণ করা হয়নি।",
        ["medSearchPlaceholder"] = "ওষুধ খুঁজুন",
        ["medStrengthPlaceholder"] = "শক্তি",
        ["medDosagePlaceholder"] = "যেমন: ১ ট্যাবলেট",
        ["medFrequencyPlaceholder"] = "যেমন: দিনে দুইবার (BD)",
        ["medDurationPlaceholder"] = "যেমন: ৭ দিন",
        ["medInstructionsPlaceholder"] = "যেমন: খাবারের পর",
        ["regNo"] = "নিবন্ধন নং",
        ["bmdcRegNo"] = "বিএমএন্ডডিসি নিবন্ধন নং",
        ["callPrefix"] = "কল",
        ["emergency"] = "জরুরি",
        ["generatedBy"] = "PrescriptionMS দ্বারা তৈরি",
        ["qrScanToVerify"] = "যাচাই করতে স্ক্যান করুন"
    };
}

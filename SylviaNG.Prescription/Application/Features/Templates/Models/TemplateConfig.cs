namespace SylviaNG.Prescription.Application.Features.Templates.Models;

/// <summary>
/// The full header/footer/style/visibility/print/label configuration for a
/// <see cref="Domain.Entities.PrescriptionTemplate"/>. Serialized as a single JSON string into
/// the entity's <c>ConfigJson</c> column (see <c>TemplateMappings</c>) rather than exploded into
/// one DB column per field — see the entity's doc comment for why.
/// </summary>
public class TemplateConfig
{
    public HeaderConfig Header { get; set; } = new();
    public FooterConfig Footer { get; set; } = new();
    public StyleConfig Style { get; set; } = new();
    public VisibilityConfig Visibility { get; set; } = new();
    public PrintConfig Print { get; set; } = new();
    public Dictionary<string, string> Labels { get; set; } = new();
}

public class HeaderConfig
{
    public string? BgColor { get; set; }
    public int Height { get; set; }
    public int LogoSize { get; set; }
    /// <summary>"heading" | "body"</summary>
    public string NameFont { get; set; } = "heading";
    /// <summary>"none" | "solid" | "dashed"</summary>
    public string BorderStyle { get; set; } = "none";
}

public class FooterConfig
{
    public string? BgColor { get; set; }
    public int Height { get; set; }
    public string QrMessage { get; set; } = string.Empty;
    public string QrMessageBn { get; set; } = string.Empty;
    /// <summary>"none" | "solid" | "dashed"</summary>
    public string BorderStyle { get; set; } = "none";
}

public class StyleConfig
{
    public int SectionSpacing { get; set; }
    public int BorderRadius { get; set; }
    /// <summary>"none" | "solid" | "dashed"</summary>
    public string BorderStyle { get; set; } = "none";
    public string? AccentColor { get; set; }
    /// <summary>"heading" | "body" | "bangla"</summary>
    public string FontFamily { get; set; } = "body";
    public int FontSize { get; set; }
    /// <summary>"plain" | "striped" | "bordered"</summary>
    public string TableStyle { get; set; } = "plain";
    public string? WatermarkText { get; set; }
}

public class VisibilityConfig
{
    public bool Logo { get; set; } = true;
    public bool Slogan { get; set; } = true;
    public bool Footer { get; set; } = true;
    public bool Watermark { get; set; } = false;
}

public class PrintConfig
{
    public string PageSize { get; set; } = "A4";
    /// <summary>"portrait" | "landscape"</summary>
    public string Orientation { get; set; } = "portrait";
    public int MarginMm { get; set; }
}

namespace Richie.Application.Common;

/// <summary>
/// The central Richie brand palette (adapted from the design system). Values are sRGB hex strings so
/// any layer can convert to its own colour type (WPF <c>Media.Color</c>, SkiaSharp <c>SKColor</c>).
/// The categorical list is colour-blind-safe and is the single source of truth for chart series
/// colours on-screen and in exported reports. No "Others" bucket — every series is named explicitly.
/// </summary>
public static class BrandColors
{
    public const string Primary = "#E6A756";   // Soft Golden Orange (application primary accent)
    public const string Secondary = "#5B8DEF"; // Professional Blue (secondary accent)
    public const string Accent = "#9B7EDE";    // Investment purple (accent)

    // Status triad (consistent app-wide): green = good, amber = needs attention, red = critical.
    public const string Success = "#57B894";
    public const string Warning = "#F3C969";
    public const string Danger = "#D96C6C";

    /// <summary>Distinct, colour-blind-safe categorical series colours (dashboard order).</summary>
    public static readonly IReadOnlyList<string> Categorical =
    [
        "#5B8DEF", // Equity / Professional Blue
        "#E6A756", // Mutual Fund / Soft Golden Orange
        "#57B894", // SGB / Green
        "#9B7EDE", // Digital Gold / Purple
        "#56B7B1", // Real Estate / Teal
        "#F3C969", // Jewellery / Soft Yellow
        "#8A8A8A", // Guaranteed Plans / Gray
        "#C19C6B", // filler
        "#7F6FAD", // filler
        "#5A8A6B"  // filler
    ];
}

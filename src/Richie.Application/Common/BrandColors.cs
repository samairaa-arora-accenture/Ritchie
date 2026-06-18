namespace Richie.Application.Common;

/// <summary>
/// The central Richie brand palette (adapted from the design system). Values are sRGB hex strings so
/// any layer can convert to its own colour type (WPF <c>Media.Color</c>, SkiaSharp <c>SKColor</c>).
/// The categorical list is colour-blind-safe and is the single source of truth for chart series
/// colours on-screen and in exported reports. No "Others" bucket — every series is named explicitly.
/// </summary>
public static class BrandColors
{
    public const string Primary = "#2926c9";   // Richie Red
    public const string Secondary = "#45a5d8"; // Golden Amber
    public const string Accent = "#fbfaf7";    // Soft Gold

    // Status triad (consistent app-wide): green = good, amber = needs attention, red = critical.
    // Updated to remove green/red while keeping a clear status distinction.
    public const string Success = "#25eb3f"; // blue
    public const string Warning = "#DE9326";
    public const string Danger = "#ed523a"; // violet


    /// <summary>Distinct, colour-blind-safe categorical series colours.</summary>
    public static readonly IReadOnlyList<string> Categorical =
    [
        "#2563EB", // Royal Blue
        "#D97706", // Amber
        "#7C3AED", // Purple
        "#EA580C", // Orange
        "#3B82F6", // Medium Blue
        "#6D28D9", // Deep Purple
        "#EAB308", // Yellow
        "#4F46E5", // Indigo
        "#64748B", // Slate
        "#1E3A8A"  // Navy Blue
    ];
}

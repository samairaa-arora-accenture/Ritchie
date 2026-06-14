namespace Richie.Domain.Settings;

/// <summary>Per-user application settings (PRD §15). One row per user.</summary>
public class AppSettings
{
    public Guid UserId { get; set; }

    /// <summary>"Light" | "Dark" | "System".</summary>
    public string Theme { get; set; } = "System";

    /// <summary>Inactivity auto-lock timeout, in minutes.</summary>
    public int SessionLockMinutes { get; set; } = 5;

    /// <summary>Global include/exclude of gold jewellery in portfolio valuation (PRD §6.10/§15).</summary>
    public bool IncludeJewelleryInPortfolio { get; set; } = true;

    /// <summary>"Manual" | "Daily" | "Weekly".</summary>
    public string BackupFrequency { get; set; } = "Manual";

    /// <summary>Comma-separated <c>NotificationType</c> names the user has switched off.</summary>
    public string DisabledNotificationTypes { get; set; } = string.Empty;

    public DateTime? LastBackupUtc { get; set; }
}

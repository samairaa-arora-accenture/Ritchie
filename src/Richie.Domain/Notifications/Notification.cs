namespace Richie.Domain.Notifications;

/// <summary>Notification triggers (PRD §13). Only <see cref="SipPosted"/> is raised so far.</summary>
public enum NotificationType
{
    SipReminder = 1,
    SipPosted = 2,
    RecurringExpense = 3,
    InsuranceRenewal = 4,
    PortfolioHealthAlert = 5,
    ExpenseAlert = 6,
    UploadStatus = 7,
    GipMaturity = 8
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}

namespace Richie.Domain.Assets;

/// <summary>
/// A recurring investment plan attached to one asset (PRD §6.8). When <see cref="NextRunDateUtc"/>
/// is reached the system auto-adds <see cref="Amount"/> to the asset and schedules the next run.
/// </summary>
public class SipSchedule
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Guid UserId { get; set; }

    public bool IsEnabled { get; set; }
    public decimal Amount { get; set; }
    public int DayOfMonth { get; set; }          // 1–28
    public SipFrequency Frequency { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime NextRunDateUtc { get; set; }
    public DateTime? LastRunUtc { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

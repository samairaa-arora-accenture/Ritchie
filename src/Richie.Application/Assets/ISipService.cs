namespace Richie.Application.Assets;

/// <summary>
/// SIP configuration per asset plus the automation that auto-posts due instalments (PRD §6.8).
/// </summary>
public interface ISipService
{
    SipScheduleDto? GetSchedule(Guid assetId);
    void SaveSchedule(Guid assetId, SipScheduleInput input);

    IReadOnlyList<DateTime> GetUpcomingInstallments(Guid assetId, int count = 4);
    IReadOnlyList<SipContributionDto> GetHistory(Guid assetId);

    /// <summary>
    /// Posts every instalment due at or before <paramref name="nowUtc"/> across ALL users
    /// (run by the background service). Adds the amount to each asset, records the contribution,
    /// raises a notification, and advances the schedule. Returns the number of instalments posted.
    /// </summary>
    int ProcessDueSips(DateTime nowUtc);
}

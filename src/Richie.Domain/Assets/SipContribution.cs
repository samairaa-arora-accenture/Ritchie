namespace Richie.Domain.Assets;

/// <summary>One auto-posted SIP instalment — the SIP history (PRD §6.8).</summary>
public class SipContribution
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Guid SipScheduleId { get; set; }
    public DateTime DateUtc { get; set; }
    public decimal Amount { get; set; }
}

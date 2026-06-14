using Microsoft.EntityFrameworkCore;
using Richie.Application.Abstractions;
using Richie.Application.Authentication;
using Richie.Application.Insurance;
using Richie.Domain.Auditing;
using Richie.Domain.Insurance;
using Richie.Domain.Notifications;
using Richie.Infrastructure.Auditing;
using Richie.Infrastructure.Notifications;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Insurance;

public sealed class InsuranceService : IInsuranceService
{
    private const string Module = "Insurance";

    /// <summary>Renewal reminder lead time. (PRD §10: 30 days before renewal.)</summary>
    private static readonly TimeSpan RenewalLeadTime = TimeSpan.FromDays(30);

    private readonly IAppDbContextFactory _factory;
    private readonly IUserSession _session;
    private readonly IClock _clock;

    public InsuranceService(IAppDbContextFactory factory, IUserSession session, IClock clock)
    {
        _factory = factory;
        _session = session;
        _clock = clock;
    }

    private Guid UserId => _session.UserId ?? throw new InvalidOperationException("No authenticated user.");

    public IReadOnlyList<InsurancePolicySummary> GetPolicies()
    {
        Guid userId = UserId;
        DateTime now = _clock.UtcNow;

        using RichieDbContext db = _factory.Create();
        return db.InsurancePolicies.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.RenewalDate)
            .ToList()
            .Select(p => ToSummary(p, now))
            .ToList();
    }

    public InsurancePolicyInput? GetById(Guid id)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        InsurancePolicy? p = db.InsurancePolicies.AsNoTracking().FirstOrDefault(x => x.Id == id && x.UserId == userId);
        return p is null ? null : new InsurancePolicyInput(p.Type, p.PolicyName, p.PolicyNumber, p.Provider,
            p.CoverageAmount, p.AnnualPremium, p.StartDate, p.RenewalDate, p.Nominee, p.Notes);
    }

    public Guid Create(InsurancePolicyInput input)
    {
        Guid userId = UserId;
        DateTime now = _clock.UtcNow;

        var policy = new InsurancePolicy
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = input.Type,
            PolicyName = input.PolicyName.Trim(),
            PolicyNumber = Trim(input.PolicyNumber),
            Provider = Trim(input.Provider),
            CoverageAmount = input.CoverageAmount,
            AnnualPremium = input.AnnualPremium,
            StartDate = input.StartDate,
            RenewalDate = input.RenewalDate,
            Nominee = Trim(input.Nominee),
            Notes = Trim(input.Notes),
            CreatedUtc = now,
            UpdatedUtc = now
        };

        using RichieDbContext db = _factory.Create();
        db.InsurancePolicies.Add(policy);
        AuditWriter.Add(db, userId, now, Module, AuditAction.Create, nameof(InsurancePolicy), policy.Id,
            $"Added {InsuranceTypeNames.Display(policy.Type)} policy '{policy.PolicyName}'.");
        db.SaveChanges();
        return policy.Id;
    }

    public bool Update(Guid id, InsurancePolicyInput input)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        InsurancePolicy? p = db.InsurancePolicies.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (p is null)
            return false;

        bool renewalChanged = p.RenewalDate != input.RenewalDate;

        p.Type = input.Type;
        p.PolicyName = input.PolicyName.Trim();
        p.PolicyNumber = Trim(input.PolicyNumber);
        p.Provider = Trim(input.Provider);
        p.CoverageAmount = input.CoverageAmount;
        p.AnnualPremium = input.AnnualPremium;
        p.StartDate = input.StartDate;
        p.RenewalDate = input.RenewalDate;
        p.Nominee = Trim(input.Nominee);
        p.Notes = Trim(input.Notes);
        p.UpdatedUtc = _clock.UtcNow;

        // Re-arm the renewal reminder if the renewal date moved.
        if (renewalChanged)
            p.RenewalNotifiedForDate = null;

        AuditWriter.Add(db, userId, p.UpdatedUtc, Module, AuditAction.Update, nameof(InsurancePolicy), p.Id,
            $"Updated policy '{p.PolicyName}'.");
        db.SaveChanges();
        return true;
    }

    public bool Delete(Guid id)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        InsurancePolicy? p = db.InsurancePolicies.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (p is null)
            return false;

        db.InsurancePolicies.Remove(p);
        AuditWriter.Add(db, userId, _clock.UtcNow, Module, AuditAction.Delete, nameof(InsurancePolicy), p.Id,
            $"Deleted policy '{p.PolicyName}'.");
        db.SaveChanges();
        return true;
    }

    public int ProcessDueRenewals(DateTime nowUtc)
    {
        DateTime threshold = nowUtc + RenewalLeadTime;

        using RichieDbContext db = _factory.Create();
        // Due = renewal on/before the lead threshold and not already notified for this renewal date.
        List<InsurancePolicy> due = db.InsurancePolicies
            .Where(p => p.RenewalDate <= threshold &&
                        (p.RenewalNotifiedForDate == null || p.RenewalNotifiedForDate != p.RenewalDate))
            .ToList();

        int raised = 0;
        foreach (InsurancePolicy p in due)
        {
            int days = (int)Math.Ceiling((p.RenewalDate - nowUtc).TotalDays);
            string when = days <= 0 ? "is due" : $"renews in {days} day(s)";
            NotificationWriter.Add(db, p.UserId, nowUtc, NotificationType.InsuranceRenewal,
                "Insurance renewal",
                $"Your {InsuranceTypeNames.Display(p.Type)} policy '{p.PolicyName}' {when} ({p.RenewalDate:d}).");
            p.RenewalNotifiedForDate = p.RenewalDate;
            AuditWriter.Add(db, p.UserId, nowUtc, Module, AuditAction.Update, nameof(InsurancePolicy), p.Id,
                "Raised renewal reminder.");
            raised++;
        }

        if (raised > 0)
            db.SaveChanges();
        return raised;
    }

    private static InsurancePolicySummary ToSummary(InsurancePolicy p, DateTime now) => new(
        p.Id, p.Type, InsuranceTypeNames.Display(p.Type), p.PolicyName, p.Provider,
        p.CoverageAmount, p.AnnualPremium, p.RenewalDate, (int)Math.Ceiling((p.RenewalDate - now).TotalDays));

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

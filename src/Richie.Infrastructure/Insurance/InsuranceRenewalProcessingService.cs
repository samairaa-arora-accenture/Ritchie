using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Richie.Application.Abstractions;
using Richie.Application.Insurance;

namespace Richie.Infrastructure.Insurance;

/// <summary>
/// Background job that raises insurance-renewal reminders (PRD §10/§13: 30 days before renewal).
/// Runs once at startup, then periodically. Mirrors the SIP/recurring processors; migrations run
/// before the host starts, so the first pass is safe.
/// </summary>
public sealed class InsuranceRenewalProcessingService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    private readonly IInsuranceService _insurance;
    private readonly IClock _clock;
    private readonly ILogger<InsuranceRenewalProcessingService> _logger;

    public InsuranceRenewalProcessingService(
        IInsuranceService insurance, IClock clock, ILogger<InsuranceRenewalProcessingService> logger)
    {
        _insurance = insurance;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int raised = _insurance.ProcessDueRenewals(_clock.UtcNow);
                if (raised > 0)
                    _logger.LogInformation("Raised {Count} insurance-renewal reminder(s).", raised);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Insurance renewal processing failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}

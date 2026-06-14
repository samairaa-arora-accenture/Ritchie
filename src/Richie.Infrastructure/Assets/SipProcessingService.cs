using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Richie.Application.Abstractions;
using Richie.Application.Assets;

namespace Richie.Infrastructure.Assets;

/// <summary>
/// Background job that auto-posts due SIP instalments (PRD §6.8). Runs once at startup, then on
/// a periodic interval. The database is migrated before the host starts, so the first run is safe.
/// </summary>
public sealed class SipProcessingService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    private readonly ISipService _sip;
    private readonly IClock _clock;
    private readonly ILogger<SipProcessingService> _logger;

    public SipProcessingService(ISipService sip, IClock clock, ILogger<SipProcessingService> logger)
    {
        _sip = sip;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int posted = _sip.ProcessDueSips(_clock.UtcNow);
                if (posted > 0)
                    _logger.LogInformation("Auto-posted {Count} SIP instalment(s).", posted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SIP processing failed.");
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

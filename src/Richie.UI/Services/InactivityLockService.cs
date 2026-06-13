using System.Windows.Threading;

namespace Richie.UI.Services;

/// <summary>
/// Locks the app after a period of no user input. The shell calls <see cref="Notify"/> on
/// input; if no activity occurs within <see cref="Timeout"/>, <see cref="Locked"/> fires and
/// App returns to the login screen. Timeout becomes configurable via Settings (PRD §15) later.
/// </summary>
public sealed class InactivityLockService
{
    private readonly DispatcherTimer _timer;
    private DateTime _lastActivityUtc;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    public event EventHandler? Locked;

    public InactivityLockService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _timer.Tick += OnTick;
    }

    public void Start()
    {
        _lastActivityUtc = DateTime.UtcNow;
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    public void Notify() => _lastActivityUtc = DateTime.UtcNow;

    private void OnTick(object? sender, EventArgs e)
    {
        if (DateTime.UtcNow - _lastActivityUtc < Timeout)
            return;

        Stop();
        Locked?.Invoke(this, EventArgs.Empty);
    }
}

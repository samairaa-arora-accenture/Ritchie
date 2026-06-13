namespace Richie.UI.Services;

/// <summary>
/// Singleton channel for requesting the first-run app tour. The main shell listens and
/// shows the tour overlay; the Help page (and first login) request it.
/// </summary>
public sealed class TourService
{
    public event Action? StartRequested;

    public void Request() => StartRequested?.Invoke();
}

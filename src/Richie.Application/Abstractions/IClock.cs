namespace Richie.Application.Abstractions;

/// <summary>
/// Abstraction over the current time so time-dependent logic (lockouts, scheduling)
/// is deterministic in tests.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

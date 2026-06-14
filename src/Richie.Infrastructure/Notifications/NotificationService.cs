using Microsoft.EntityFrameworkCore;
using Richie.Application.Authentication;
using Richie.Application.Notifications;
using Richie.Application.Settings;
using Richie.Domain.Notifications;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly IAppDbContextFactory _factory;
    private readonly IUserSession _session;
    private readonly IAppSettingsService _settings;

    public NotificationService(IAppDbContextFactory factory, IUserSession session, IAppSettingsService settings)
    {
        _factory = factory;
        _session = session;
        _settings = settings;
    }

    private Guid UserId => _session.UserId ?? throw new InvalidOperationException("No authenticated user.");

    public IReadOnlyList<NotificationDto> GetRecent(int count = 20)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        return db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedUtc)
            .ToList()
            .Where(n => _settings.IsNotificationEnabled(n.Type))   // honour per-type prefs (PRD §15)
            .Take(count)
            .Select(n => new NotificationDto(n.Id, n.CreatedUtc, n.Type, n.Title, n.Message, n.IsRead))
            .ToList();
    }

    public int GetUnreadCount()
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        return db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToList()
            .Count(n => _settings.IsNotificationEnabled(n.Type));
    }

    public void MarkAllRead()
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        foreach (var n in db.Notifications.Where(n => n.UserId == userId && !n.IsRead))
            n.IsRead = true;
        db.SaveChanges();
    }

    public void MarkRead(Guid id)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        Notification? n = db.Notifications.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (n is not null && !n.IsRead)
        {
            n.IsRead = true;
            db.SaveChanges();
        }
    }

    public void Dismiss(Guid id)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        Notification? n = db.Notifications.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (n is not null)
        {
            db.Notifications.Remove(n);
            db.SaveChanges();
        }
    }
}

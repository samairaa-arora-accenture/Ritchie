using Microsoft.EntityFrameworkCore;
using Richie.Application.Authentication;
using Richie.Application.Notifications;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly IAppDbContextFactory _factory;
    private readonly IUserSession _session;

    public NotificationService(IAppDbContextFactory factory, IUserSession session)
    {
        _factory = factory;
        _session = session;
    }

    private Guid UserId => _session.UserId ?? throw new InvalidOperationException("No authenticated user.");

    public IReadOnlyList<NotificationDto> GetRecent(int count = 20)
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        return db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedUtc)
            .Take(count)
            .Select(n => new NotificationDto(n.Id, n.CreatedUtc, n.Type, n.Title, n.Message, n.IsRead))
            .ToList();
    }

    public int GetUnreadCount()
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        return db.Notifications.Count(n => n.UserId == userId && !n.IsRead);
    }

    public void MarkAllRead()
    {
        Guid userId = UserId;
        using RichieDbContext db = _factory.Create();
        foreach (var n in db.Notifications.Where(n => n.UserId == userId && !n.IsRead))
            n.IsRead = true;
        db.SaveChanges();
    }
}

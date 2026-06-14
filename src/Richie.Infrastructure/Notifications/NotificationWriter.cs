using Richie.Domain.Notifications;
using Richie.Infrastructure.Persistence;

namespace Richie.Infrastructure.Notifications;

/// <summary>Adds a notification to a context so it saves in the same transaction as its trigger.</summary>
public static class NotificationWriter
{
    public static void Add(RichieDbContext db, Guid userId, DateTime nowUtc,
        NotificationType type, string title, string message)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedUtc = nowUtc,
            Type = type,
            Title = title,
            Message = message,
            IsRead = false
        });
    }
}

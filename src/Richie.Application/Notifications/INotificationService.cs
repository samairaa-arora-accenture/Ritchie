using Richie.Domain.Notifications;

namespace Richie.Application.Notifications;

public sealed record NotificationDto(
    Guid Id, DateTime CreatedUtc, NotificationType Type, string Title, string Message, bool IsRead);

/// <summary>Read access to the signed-in user's notifications (creation is done by the system).</summary>
public interface INotificationService
{
    IReadOnlyList<NotificationDto> GetRecent(int count = 20);
    int GetUnreadCount();
    void MarkAllRead();
}

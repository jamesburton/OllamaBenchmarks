using System;
using System.Collections.Generic;
using System.Linq;

public record Notification(string Id, string UserId, string Message, bool IsRead);

public class NotificationCenter
{
    private readonly Dictionary<string, Notification> _notifications = new();

    public void Send(Notification notification)
    {
        if (notification == null)
        {
            throw new ArgumentNullException(nameof(notification));
        }
        _notifications[notification.Id] = notification;
    }

    public void MarkAsRead(string notificationId)
    {
        if (_notifications.TryGetValue(notificationId, out var existingNotification))
        {
            _notifications[notificationId] = existingNotification with { IsRead = true };
        }
    }

    public List<Notification> GetUnread(string userId)
    {
        return _notifications.Values
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToList();
    }

    public int UnreadCount(string userId)
    {
        return _notifications.Values
            .Count(n => n.UserId == userId && !n.IsRead);
    }
}
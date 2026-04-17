using System.Threading.Tasks;

public interface INotificationSender
{
    Task SendAsync(string recipient, string message);
}

public class ReminderService
{
    private readonly INotificationSender _sender;

    public ReminderService(INotificationSender sender)
    {
        _sender = sender;
    }

    public async Task SendReminderAsync(string userId, string task)
    {
        await _sender.SendAsync(userId, $"Reminder: {task}");
    }
}
using System.Threading.Tasks;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class NotificationService
{
    private readonly IEmailSender _sender;

    public NotificationService(IEmailSender sender)
    {
        _sender = sender;
    }

    public async Task NotifyAsync(string email, string message)
    {
        await _sender.SendAsync(email, "Notification", message);
    }
}
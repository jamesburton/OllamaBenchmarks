public interface UserRepository
{
    Task<User?> Get(int id);
}

public interface IEmailService
{
    Task Send(string email) => await _email.SendAsync(email).GetAwaiter().GetResult();
}

public record User { public int Id; public string Email; }

public class NotificationService(IUser Repository, IEmail Service)
{
    public async Task Notify(int userId)
    {
        var user = await Repository.Get(userId);
        if (user == null) throw new InvalidOperationException("User not found");
        await Service.Send(user.Email);
    }
}
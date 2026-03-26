public class NotificationService : IUserRepository, IEmailService
{
    public async Task NotifyUserAsync(int userId)
    {
        var user = await GetUserById(userId);
        await email.SendWelcomeAsync(user.Email);
    }
}

[Fact]
public async Task<Task<User?>> ExpectResult(User? user)
{
    var repo = new IUserRepository();
    var email = new IEmailService();
    var service = new NotificationService(repo, email);
    var user = await service.NotifyUserAsync(1);
    Assert.Equal(user?.Id, 1);
    Assert.Equal(user?.Name, "Test");
    Assert.Equal(user?.Email, "TestEmail");
    return Task.FromResult(ExpectedResult);
}
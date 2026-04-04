public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task SaveAsync(User user);
}

public interface IEmailService
{
    Task SendWelcomeAsync(string email);
}

public record User
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
}

public class NotificationService(IUserRepository repo, IEmailService email)
{
    public async Task NotifyUserAsync(int userId)
    {
        var user = await repo.GetByIdAsync(userId) ??
                     throw new InvalidOperationException("User not found");
        await email.SendWelcomeAsync(user.Email);
    }
}

public class NotificationTests
{
    [Fact]
    public async Task HappyPath_EmpsMailSent()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, email);
        int userId = 42;
        var user = new User { Id = userId, Name = "Tester", Email = "test@example.com" };
        repo.GetByIdAsync(userId).Returns(user);
        email.SendWelcomeAsync("test@example.com").Returns(Task.CompletedTask);
        var act = () => sut.NotifyUserAsync(userId);
        await act.Should().BeVoid();
        repo.Received().GetUserByIdAsync(userId);
        email.Received().SendWelcomeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task UserNotFound_ThrowsInvalidOperationException()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, email);
        int userId = 99;
        repo.GetByIdAsync(userId).Returns(Task.CompletedTask);
        email.Received().SendWelcomeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
        var act = () => sut.NotifyUserAsync(userId);
        await act.Should().ThrowAsync<InvalidOperationException>();
        repo.Received().GetUserByIdAsync(userId);
    }
}
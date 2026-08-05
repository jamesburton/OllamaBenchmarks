using AwesomeAssertions;
using NSubstitute;

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
        var user = await repo.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");
        await email.SendWelcomeAsync(user.Email);
    }
}

class TestNotificationService
{
    [Fact]
    async Task NotifyUser_WhenUserExists_SendsWelcomeEmail()
    {
        var repoMock = Substitute.For<IUserRepository>();
        var emailMock = Substitute.For<IEmailService>();
        var sut = new NotificationService(repoMock, emailMock);

        User user = new() { Id = 1, Name = "Alice", Email = "alice@example.com" };
        repoMock.GetByIdAsync(1).Returns(user);

        await sut.NotifyUserAsync(1);

        emailMock.SendWelcomeAsync(user.Email).Should().BeVoid();
    }

    [Fact]
    async Task NotifyUser_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        var repoMock = Substitute.For<IUserRepository>();
        var sut = new NotificationService(repoMock, null);

        await repositoryMock.GetByIdAsync(999).That.Can.Throw();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.NotifyUserAsync(999));
    }

    [Fact]
    void VerifyReceivedCalls_IUserRepository()
    {
        var repoMock = Substitute.For<IUserRepository>();
        var emailMock = Substitute.For<IEmailService>();
        var sut = new NotificationService(repoMock, emailMock);

        User user = new() { Id = 1, Name = "Bob", Email = "bob@example.com" };
        repoMock.GetByIdAsync(1).Returns(user);
        await sut.NotifyUserAsync(1);

        repoMock.Received().GetByIdAsync(1);
    }
}
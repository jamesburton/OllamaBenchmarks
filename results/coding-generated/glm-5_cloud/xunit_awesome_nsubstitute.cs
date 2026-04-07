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

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_WhenUserExists_SendsWelcomeEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, emailService);

        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        repo.GetByIdAsync(user.Id).Returns(user);

        // Act
        await sut.NotifyUserAsync(user.Id);

        // Assert
        await repo.Received().GetByIdAsync(user.Id);
        await emailService.Received().SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, emailService);

        repo.GetByIdAsync(Arg.Any<int>()).Returns((User?)null);

        // Act
        Func<Task> act = () => sut.NotifyUserAsync(999);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}
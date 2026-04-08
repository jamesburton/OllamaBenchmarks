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
    public async Task NotifyUserAsync_UserExists_SendsWelcomeEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);
        var user = new User { Id = 1, Name = "Test", Email = "test@example.com" };
        repo.GetByIdAsync(1).Returns(user);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await repo.Received(1).GetByIdAsync(1);
        await email.Received(1).SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);
        repo.GetByIdAsync(Arg.Any<int>()).Returns((User?)null);

        // Act
        var act = async () => await service.NotifyUserAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await repo.Received(1).GetByIdAsync(1);
        await email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}
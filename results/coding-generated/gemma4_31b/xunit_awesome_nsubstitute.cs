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
        var sut = new NotificationService(repo, email);

        var userId = 1;
        var user = new User { Id = userId, Name = "John Doe", Email = "john@example.com" };

        repo.GetByIdAsync(userId).Returns(user);

        // Act
        await sut.NotifyUserAsync(userId);

        // Assert
        await email.Received().SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, email);

        int userId = 99;
        repo.GetByIdAsync(userId).Returns((User?)null);

        // Act
        Func<Task> act = () => sut.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}
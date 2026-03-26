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
    public async Task NotifyUserAsync_HappyPath_SendsWelcomeEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var expectedUser = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
        repo.GetByIdAsync(1).Returns(expectedUser);

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        email.Received().SendWelcomeAsync(expectedUser.Email);
        repo.Received().GetByIdAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        repo.GetByIdAsync(1).Returns((User?)null);

        var service = new NotificationService(repo, email);

        // Act
        Func<Task> act = () => service.NotifyUserAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        repo.Received().GetByIdAsync(1);
        email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}
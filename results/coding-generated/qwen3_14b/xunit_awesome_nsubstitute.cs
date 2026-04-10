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

[Fact]
public async Task NotifyUserAsync_ShouldSendWelcomeEmail_WhenUserIsFound()
{
    // Arrange
    var user = new User { Id = 1, Email = "test@example.com" };
    var repoMock = Substitute.For<IUserRepository>();
    repoMock.GetByIdAsync(1).ReturnsAsync(user);
    var emailMock = Substitute.For<IEmailService>();
    emailMock.SendWelcomeAsync("test@example.com").Returns(Task.CompletedTask);

    var service = new NotificationService(repoMock, emailMock);

    // Act
    await service.NotifyUserAsync(1);

    // Assert
    await repoMock.Received().GetByIdAsync(1);
    await emailMock.Received().SendWelcomeAsync("test@example.com");
}

[Fact]
public async Task NotifyUserAsync_ShouldThrowException_WhenUserIsNotFound()
{
    // Arrange
    var repoMock = Substitute.For<IUserRepository>();
    repoMock.GetByIdAsync(2).ReturnsAsync((User?)null);
    var emailMock = Substitute.For<IEmailService>();
    var service = new NotificationService(repoMock, emailMock);

    // Act & Assert
    var act = async () => await service.NotifyUserAsync(2);
    await act.Should().ThrowAsync<InvalidOperationException>();

    // Verify that GetById was called, but SendWelcome was not
    await repoMock.Received().GetByIdAsync(2);
    emailMock.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
}
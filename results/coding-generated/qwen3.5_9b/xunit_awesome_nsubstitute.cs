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
public void NotifyUserAsync_HappyPath()
{
    // Arrange
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    var expectedUser = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
    mockRepo.GetByIdAsync(1).Returns(Task.FromResult(expectedUser));
    mockEmail.SendWelcomeAsync("alice@example.com").Returns(Task.CompletedTask);

    var sut = new NotificationService(mockRepo, mockEmail);

    // Act & Assert
    act.Should().NotThrow();
}

[Fact]
public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
{
    // Arrange
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(99).Returns(Task.FromResult<User?>(null));

    var sut = new NotificationService(mockRepo, mockEmail);

    // Act & Assert
    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("User not found");
}

[Fact]
public async Task NotifyUserAsync_VerifyCalls()
{
    // Arrange
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    var expectedUser = new User { Id = 1, Name = "Bob", Email = "bob@example.com" };
    mockRepo.GetByIdAsync(1).Returns(Task.FromResult(expectedUser));

    var sut = new NotificationService(mockRepo, mockEmail);

    // Act
    await act;

    // Assert - Verify calls on mocks
    mockRepo.Received().GetByIdAsync(Arg.Any<int>());
    mockEmail.Received().SendWelcomeAsync("bob@example.com");
}
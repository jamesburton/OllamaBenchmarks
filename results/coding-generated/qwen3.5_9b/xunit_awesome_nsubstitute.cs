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
public async Task NotifyUserAsync_HappyPath()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
    mockRepo.GetByIdAsync(1).Returns(Task.FromResult(user));
    mockEmail.SendWelcomeAsync("alice@example.com").Returns(Task.CompletedTask);

    var sut = new NotificationService(mockRepo, mockEmail);

    await sut.NotifyUserAsync(1);

    mockRepo.Received(1).GetByIdAsync(1);
    mockEmail.Received(1).SendWelcomeAsync("alice@example.com");
}

[Fact]
public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(99).Returns(Task.FromResult<User?>(null));

    var sut = new NotificationService(mockRepo, mockEmail);

    await act.Should().ThrowAsync<InvalidOperationException>();
}

[Fact]
public async Task NotifyUserAsync_VerifyCalls()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    var user = new User { Id = 1, Name = "Bob", Email = "bob@example.com" };
    mockRepo.GetByIdAsync(1).Returns(Task.FromResult(user));
    mockEmail.SendWelcomeAsync("bob@example.com").Returns(Task.CompletedTask);

    var sut = new NotificationService(mockRepo, mockEmail);

    await sut.NotifyUserAsync(1);

    Received.InOrder(() =>
    {
        mockRepo.Received(1).GetByIdAsync(1);
        mockEmail.Received(1).SendWelcomeAsync("bob@example.com");
    });
}
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
    public async Task NotifyUserAsync_HappyPath_UserFound_EmailSent()
    {
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
        mockRepo.GetByIdAsync(1).Returns(Task.FromResult(user));

        var sut = new NotificationService(mockRepo, mockEmail);

        await sut.NotifyUserAsync(1);

        mockRepo.Received(1).GetByIdAsync(Arg.Any<int>());
        mockEmail.Received(1).SendWelcomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        mockRepo.GetByIdAsync(999).Returns(Task.FromResult<User?>(null));

        var sut = new NotificationService(mockRepo, mockEmail);

        var act = async () => await sut.NotifyUserAsync(999);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task NotifyUserAsync_VerifiesCallOrder()
    {
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        var user = new User { Id = 42, Name = "Bob", Email = "bob@example.com" };
        mockRepo.GetByIdAsync(42).Returns(Task.FromResult(user));

        var sut = new NotificationService(mockRepo, mockEmail);

        await sut.NotifyUserAsync(42);

        Received.InOrder(() =>
        {
            mockRepo.GetByIdAsync(Arg.Any<int>());
            mockEmail.SendWelcomeAsync(Arg.Any<string>());
        });
    }
}
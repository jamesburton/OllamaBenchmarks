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
    public async Task NotifyUserAsync_UserFound_SendsEmailWithCorrectAddress()
    {
        var userId = 1;
        var userEmail = "test@example.com";
        var user = new User { Id = userId, Email = userEmail };

        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        mockRepo.GetByIdAsync(userId).Returns(user);

        var sut = new NotificationService(mockRepo, mockEmail);

        await sut.NotifyUserAsync(userId);

        await mockRepo.Received(1).GetByIdAsync(userId);
        await mockEmail.Received(1).SendWelcomeAsync(userEmail);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var userId = 999;

        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        mockRepo.GetByIdAsync(userId).Returns((User?)null);

        var sut = new NotificationService(mockRepo, mockEmail);

        Func<Task> act = () => sut.NotifyUserAsync(userId);

        await act.Should().ThrowAsync<InvalidOperationException>();

        await mockRepo.Received(1).GetByIdAsync(userId);
        await mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}
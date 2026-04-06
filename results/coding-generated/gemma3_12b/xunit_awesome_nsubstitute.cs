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
    var expectedEmail = "test@example.com";
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    var user = new User { Id = 1, Name = "Test User", Email = expectedEmail };
    mockRepo.GetByIdAsync(1).Returns(user);

    var service = new NotificationService(mockRepo, mockEmail);
    await service.NotifyUserAsync(1);

    mockEmail.Received().SendWelcomeAsync(expectedEmail);
}

[Fact]
public async Task NotifyUserAsync_UserNotFound()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(1).Returns((User?)null);

    var service = new NotificationService(mockRepo, mockEmail);
    var act = () => service.NotifyUserAsync(1);

    act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("User not found");

    mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
}

[Fact]
public async Task NotifyUserAsync_VerifyRepoCall()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    var service = new NotificationService(mockRepo, mockEmail);
    await service.NotifyUserAsync(1);

    mockRepo.Received().GetByIdAsync(1);
}
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
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(userRepository, emailService);

        var testUser = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        userRepository.GetByIdAsync(1).Returns(testUser);

        await sut.NotifyUserAsync(1);

        await emailService.Received(1).SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(userRepository, emailService);

        userRepository.GetByIdAsync(1).Returns((User?)null);

        Func<Task> act = () => sut.NotifyUserAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }

    [Theory]
    [InlineData(1, "user1@test.com")]
    [InlineData(2, "user2@test.com")]
    [InlineData(3, "user3@test.com")]
    public async Task NotifyUserAsync_WithDifferentUsers_SendsToCorrectEmail(int userId, string expectedEmail)
    {
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(userRepository, emailService);

        var testUser = new User { Id = userId, Name = $"User {userId}", Email = expectedEmail };
        userRepository.GetByIdAsync(userId).Returns(testUser);

        await sut.NotifyUserAsync(userId);

        await emailService.Received(1).SendWelcomeAsync(expectedEmail);
    }

    [Fact]
    public async Task NotifyUserAsync_EmailServiceCalledExactlyOnce()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(userRepository, emailService);

        var testUser = new User { Id = 5, Name = "Single Call User", Email = "single@test.com" };
        userRepository.GetByIdAsync(5).Returns(testUser);

        await sut.NotifyUserAsync(5);

        await emailService.Received(1).SendWelcomeAsync(Arg.Any<string>());
        await emailService.DidNotReceiveWithAnyArgs().SendWelcomeAsync(default);
    }

    [Fact]
    public async Task NotifyUserAsync_RepositoryCalledWithCorrectId()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(userRepository, emailService);

        var testUser = new User { Id = 42, Name = "Repository Test", Email = "repo@test.com" };
        userRepository.GetByIdAsync(42).Returns(testUser);

        await sut.NotifyUserAsync(42);

        await userRepository.Received(1).GetByIdAsync(42);
    }
}
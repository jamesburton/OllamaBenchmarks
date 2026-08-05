using System;
using System.Threading.Tasks;

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

public class NotifyTests
{
    public NotifyTests()
    {
        _userRepo = Substitute.For<IUserRepository>();
        _emailService = Substitute.For<IEmailService>();
        _sut = new NotificationService(_userRepo, _emailService);
    }

    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly NotificationService _sut;

    [Fact]
    public async Task NotifyUserAsync_UserFound_EmailSentWithCorrectAddress()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
        _userRepo.GetByIdAsync(1).Returns(user);

        // Act
        await _sut.NotifyUserAsync(1);

        // Assert
        await _emailService.Received().SendWelcomeAsync("alice@example.com");
        _userRepo.Received().GetByIdAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _userRepo.GetByIdAsync(Arg.Any<int>()).Returns((User?)null);

        // Act
        Func<Task> act = () => _sut.NotifyUserAsync(99);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _userRepo.Received().GetByIdAsync(99);
        _emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}
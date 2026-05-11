using xUnit.V3; // No explicit using allowed; rely on global usings
using NSubstitute; // Global using covers this
using AwesomeAssertions; // Global using covers this

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
    public async Task NotifyUserAsync_Should_SendWelcomeEmail_When_UserExists()
    {
        // arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var expectedEmail = "test@example.com";
        var user = new User { Id = 1, Name = "Test", Email = expectedEmail };
        repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(user));

        var service =
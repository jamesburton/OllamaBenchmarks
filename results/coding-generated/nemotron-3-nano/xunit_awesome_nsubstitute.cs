using System.Threading;
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

public class NotificationServiceTests
{
    [Fact]
    public void NotifyUserAsync_HappyPath_SendsWelcomeEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var expectedEmail = "john.doe@example.com";
        var user = new User { Id = 1, Email = expectedEmail, Name = "John Doe" };

        repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(user));

        // Act
        var sut = new NotificationService(repo, email);
        await sut.NotifyUserAsync(1);

        // Assert
        repo.ReceivedCalls.Should().HaveCount(1);
        repo.ReceivedCalls.First().Arguments[0].Should().Be(1);

        email.ReceivedCalls.Should().HaveCount(1);
        email.ReceivedCalls.First().Arguments[0].Should().Be(expectedEmail);
    }

    [Fact]
    public void NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).
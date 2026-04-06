public interface IUserRepository { Task<User?> GetByIdAsync(int id); Task SaveAsync(User user); }
public interface IEmailService { Task SendWelcomeAsync(string email); }
public record User { public int Id { get; init; } public string Name { get; init; } = ""; public string Email { get; init; } = ""; }
public class NotificationService(IUserRepository repo, IEmailService email) { public async Task NotifyUserAsync(int userId) { var user = await repo.GetByIdAsync(userId) ?? throw new InvalidOperationException("User not found"); await email.SendWelcomeAsync(user.Email); } }

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var expectedEmail = "john@example.com";
        var user = new User { Id = 1, Name = "John", Email = expectedEmail };
        repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(user));

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await email.Received(1).SendWelcomeAsync(expectedEmail);
        repo.GetByIdAsync(1).Received(1);
        email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>()); // maybe not needed
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(99).Returns(Task.FromResult<User?>(null));

        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        // Act
        Func<Task> act = () => service.NotifyUserAsync(99);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        repo.GetByIdAsync(99).Received(1);
        email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}

repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(user));

var service = new NotificationService(repo, email);
await service.NotifyUserAsync(1);

email.Received(1).SendWelcomeAsync(expectedEmail);

email.Received(1).SendWelcomeAsync(Arg.Is<string>(s => s == expectedEmail));

email.Received(1).SendWelcomeAsync(Arg.Any<string>());

email.Received(1).SendWelcomeAsync(Arg.Any<string>());
email.Received(1).SendWelcomeAsync(Arg.Any<string>()).Should().Be(...) // not possible.
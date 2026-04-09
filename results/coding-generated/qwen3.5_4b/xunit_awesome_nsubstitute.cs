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
    public async Task NotifyUserAsync_HappyPath()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
        repo.GetByIdAsync(1).Returns(user);

        await repo.GetByIdAsync(1).Returns(user); // Wait, NSubstitute returns once per call unless configured differently. 
        // Actually, for async methods, Substitute.For<T> returns a Task that resolves to the value.
        // I need to ensure the return is set up correctly.
        // repo.GetByIdAsync(1).Returns(user) sets it up.

        await email.SendWelcomeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var sut = new NotificationService(repo, email);
        await sut.NotifyUserAsync(1);

        email.Received().SendWelcomeAsync(Arg.Any<string>());
        repo.Received().GetByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        var sut = new NotificationService(repo, email);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.NotifyUserAsync(1));
    }
}

var act = async () => await sut.NotifyUserAsync(1);
await act.Should().ThrowAsync<InvalidOperationException>();

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
    public async Task NotifyUserAsync_HappyPath()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
        repo.GetByIdAsync(1).Returns(user);
        email.SendWelcomeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var sut = new NotificationService(repo, email);
        await sut.NotifyUserAsync(1);

        email.Received().SendWelcomeAsync(Arg.Any<string>());
        repo.Received().GetByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        var sut = new NotificationService(repo, email);

        var act = async () => await sut.NotifyUserAsync(1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

... 
using Xunit;
using NSubstitute;
using AwesomeAssertions;
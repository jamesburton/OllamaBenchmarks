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
    public async Task HappyPath()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
        repo.GetByIdAsync(1).Returns(user);

        // Act
        await new NotificationService(repo, email).NotifyUserAsync(1);

        // Assert
        repo.Received().GetByIdAsync(Arg.Any<int>());
        email.Received().SendWelcomeAsync("john@example.com");
    }

    [Fact]
    public async Task UserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        // Act & Assert
        await new NotificationService(repo, email).NotifyUserAsync(1);
    }
}

[Fact]
    public async Task UserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        // Act & Assert
        await new NotificationService(repo, email).NotifyUserAsync(1);
    }

var act = () => new NotificationService(repo, email).NotifyUserAsync(1);
        await act.Should().ThrowAsync<InvalidOperationException>();

[Fact] — single test

public class MyTests(DatabaseFixture db) { }

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
    public async Task HappyPath()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
        repo.GetByIdAsync(1).Returns(user);

        // Act
        await new NotificationService(repo, email).NotifyUserAsync(1);

        // Assert - Verify calls using NSubstitute
        repo.Received().GetByIdAsync(Arg.Any<int>());
        email.Received().SendWelcomeAsync("john@example.com");
    }

    [Fact]
    public async Task UserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        // Act & Assert - Use AwesomeAssertions for exception
        var act = () => new NotificationService(repo, email).NotifyUserAsync(1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

var asyncAct = () => new NotificationService(repo, email).NotifyUserAsync(1);
        await asyncAct.Should().ThrowAsync<InvalidOperationException>();

Action act = () => sut.DoThing(null);
act.Should().Throw<ArgumentNullException>()
   .WithMessage("*param*");

Func<Task> asyncAct = () => sut.GetAsync(-1);
await asyncAct.Should().ThrowAsync<NotFoundException>();
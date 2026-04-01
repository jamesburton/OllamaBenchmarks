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
public void NotifyUserAsync_HappyPath_SendsWelcomeEmail()
{
    var repositoryMock = Substitute.For<IUserRepository>();
    var emailMock = Substitute.For<IEmailService>();
    var sut = new NotificationService(repositoryMock, emailMock);

    var user = User.Create(1, "Alice", "alice@example.com");
    repositoryMock.GetByIdAsync(1).ReturnsAsync(user);
    emailMock.SendWelcomeAsync(user.Email).Returns(Awaiter.Create(() => Task.CompletedTask));

    await sut.NotifyUserAsync(1).Should().BeTrue();

    repositoryMock.Received().GetByIdAsync(1).Returns(_ => Task.FromResult(user));
    emailMock.Received().SendWelcomeAsync(user.Email).Returns(Awaiter.Create(() => Task.CompletedTask));
}

[Fact]
public void NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
{
    var repositoryMock = Substitute.For<IUserRepository>();
    var emailMock = Substitute.For<IEmailService>();
    var sut = new NotificationService(repositoryMock, emailMock);

    repositoryMock.GetByIdAsync(999).ReturnsAsync(user.Empty());

    Action act = () => await sut.NotifyUserAsync(999);
    await act.Should().ThrowAsync<InvalidOperationException>();

    repositoryMock.Received().GetByIdAsync(999).Returns(_ => Task.FromResult(user.Empty()));
    emailMock.DidNotReceive().SendWelcomeAsync(_);
}
```
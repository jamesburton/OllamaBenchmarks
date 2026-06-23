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
public void NotifyUserAsync_HappyPath()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(1).Returns(new User { Id = 1, Name = "Alice", Email = "alice@example.com" });
    mockEmail.SendWelcomeAsync("alice@example.com").Returns(Task.CompletedTask);

    var sut = new NotificationService(mockRepo, mockEmail);

    Act: async () => await sut.NotifyUserAsync(1);

    Assert:
        await Act.Should().NotThrowAsync();

    Verify:
        mockRepo.Received().GetByIdAsync(Arg.Any<int>());
        mockEmail.Received().SendWelcomeAsync("alice@example.com");
}

[Fact]
public async Task NotifyUserAsync_ThrowsWhenUserNotFound()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(99).Returns((User?)null);

    var sut = new NotificationService(mockRepo, mockEmail);

    await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.NotifyUserAsync(99));
}

[Fact]
public async Task NotifyUserAsync_ThrowsWhenGetByIdAsyncThrows()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(1).Throws(new InvalidOperationException("Database error"));

    var sut = new NotificationService(mockRepo, mockEmail);

    await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.NotifyUserAsync(1));
}

[Fact]
public async Task NotifyUserAsync_InvokesSaveAsyncWhenUserNotFound()
{
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(1).Returns((User?)null);
    mockRepo.SaveAsync(Arg.Any<User>()).Returns(Task.CompletedTask);

    var sut = new NotificationService(mockRepo, mockEmail);

    await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.NotifyUserAsync(1));

    mockRepo.Received().SaveAsync(Arg.Any<User>());
}
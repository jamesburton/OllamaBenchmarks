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
    var user = new User { Id = 1, Name = "Test User", Email = expectedEmail };
    var repoSub = Substitute.For<IUserRepository>();
    var emailSub = Substitute.For<IEmailService>();

    repoSub.GetByIdAsync(1).Returns(user);

    var service = new NotificationService(repoSub, emailSub);

    await service.NotifyUserAsync(1);

    emailSub.Received().SendWelcomeAsync(expectedEmail);
}

[Fact]
public async Task NotifyUserAsync_UserNotFound()
{
    var repoSub = Substitute.For<IUserRepository>();
    var emailSub = Substitute.For<IEmailService>();

    repoSub.GetByIdAsync(1).Returns((User?)null);

    var service = new NotificationService(repoSub, emailSub);

    var act = async () => await service.NotifyUserAsync(1);

    act.Should().ThrowAsync<InvalidOperationException>();
}

[Fact]
public async Task NotifyUserAsync_VerifyRepoCall()
{
    var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
    var repoSub = Substitute.For<IUserRepository>();
    var emailSub = Substitute.For<IEmailService>();

    repoSub.GetByIdAsync(1).Returns(user);

    var service = new NotificationService(repoSub, emailSub);

    await service.NotifyUserAsync(1);

    repoSub.Received().GetByIdAsync(1);
}
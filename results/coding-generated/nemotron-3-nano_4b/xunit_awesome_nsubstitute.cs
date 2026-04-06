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

[Collection("UnitTests")]
public partial class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_HappyPath_SendsMail()
    {
        var mockRepo = Substitute.For<IUserRepository>();
        var mockMail = Substitute.For<IEmailService>();
        var sut = new NotificationService(mockRepo, mockMail);

        await sut.NotifyUserAsync(1);

        mockRepo.GetByIdAsync(Arg.Any<int>()).ReturnsAsync(new User
        {
            Id = 1,
            Email = "test@domain.com"
        });

        mockRepo.GetByIdAsync(1).Received().Value.Should().BeEqual();
        mockMail.SendWelcomeAsync(Arg.Any<string>()).Received().Value.Should().Be("test@domain.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var mockRepo = Substitute.For<IUserRepository>();
        var mockMail = Substitute.For<IEmailService>();
        var sut = new NotificationService(mockRepo, mockMail);

        await act().Should().ThrowAsync<InvalidOperationException>();

        mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        mockRepo.Received().Value.Should().BeTrue();
    }

    [Fact]
    public async Task NotifyUserAsync_EmailSent_ThenDisabledIfUserNotFound()
    {
        var mockRepo = Substitute.For<IUserRepository>();
        var mockMail = Substitute.For<IEmailService>();
        var sut = new NotificationService(mockRepo, mockMail);

        await act().Should().ThrowAsync<InvalidOperationException>();

        mockRepo.DidNotReceive().GetByIdAsync(1);
        mockMail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
        mockMail.Received().Value.Should().BeFalse();
    }
}
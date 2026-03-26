using Xunit;
using NSubstitute;
using AwesomeAssertions;

public interface IUserRepository { }
public interface IEmailService { }

public record User(int Id, string Name = "", string Email = "");

public class NotificationService : IDisposable
{
    private readonly IUserRepository _repo;
    private readonly IEmailService _email;

    public NotificationService(IUserRepository repo, IEmailService email)
    {
        _repo = repo;
        _email = email;
    }

    public async Task NotifyUserAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
        await _email.SendWelcomeAsync(user.Email);
    }

    public void Dispose() { }
}

public class NotificationServiceTests
{
    private IUserRepository _mockRepo;
    private IEmailService _mockEmail;

    [SetUp]
    public void SetUp()
    {
        _mockRepo = Substitute.For<IUserRepository>();
        _mockEmail = Substitute.For<IEmailService>();
    }

    [Test]
    public async Task NotifyUserAsync_UserFound_EmailSent_CorrectAddress()
    {
        var userId = 1;
        var expectedName = "John Doe";
        var expectedEmail = "john.doe@example.com";

        User testUser = new(userId, expectedName, expectedEmail);
        _mockRepo.GetByIdAsync(userId).Returns(testUser);

        await Assert.ThrowsAsync<InvalidOperationException>(() => notificationService.NotifyUserAsync(1)).IsNull();

        NotificationService service = new(_mockRepo, _mockEmail);
        await service.NotifyUserAsync(userId);

        _mockEmail.Received().SendWelcomeAsync(expectedEmail);
    }

    [Test]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var userId = 99;

        NotificationService service = new(_mockRepo, _mockEmail);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.NotifyUserAsync(userId)).IsNull();
    }

    [Test]
    public void NotifyUserAsync_UserNotFound_ReceivedCallsOnMocks()
    {
        var userId = 99;

        NotificationService service = new(_mockRepo, _mockEmail);
        await service.NotifyUserAsync(userId);

        Verify(x => x.GetByIdAsync(userId)).WasCalled();
        _mockEmail.Received().SendWelcomeAsync(Arg.Any<string>());
    }
}
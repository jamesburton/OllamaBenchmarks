using NSubstitute;
using AwesomeAssertions;

public class NotificationServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public NotificationServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _emailService = Substitute.For<IEmailService>();

        var mockRepo = Substitute.For<IUserRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>()).Returns(new User { Id = 1 });

        var mockEmail = Substitute.For<IEmailService>();
        mockEmail.SendWelcomeAsync(Arg.Any<string>()).Returns();

        _userRepository.When(x => x.GetByIdAsync(1)).Returns(mockRepo.Object);
        _emailService.When(x => x.SendWelcomeAsync("user@example.com")).Returns(mockEmail.Object);

        _service = new NotificationService(_userRepository, _emailService);
    }

    [Fact]
    public async Task NotifyUserAsync()
    {
        var userId = 1;
        var expectedName = "John Doe";
        var expectedEmail = "john.doe@example.com";

        await _service.NotifyUserAsync(userId);

        _userRepository.Received().GetByIdAsync(userId)
            .Returns(new User { Id = userId, Name = expectedName });

        _emailService.Received().SendWelcomeAsync(expectedEmail);
    }
}
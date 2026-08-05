using Xunit;
using AwesomeAssertions;
using NSubstitute;

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

public class NotificationService : IUserRepository, IEmailService
{
    private readonly ISubstitute<IUserRepository> _userRepository;
    private readonly ISubstitute<IEmailService> _emailService;

    public NotificationService(ISubstitute<IUserRepository> userRepository, ISubstitute<IEmailService> emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task NotifyUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        await _emailService.SendWelcomeAsync(user.Email);
    }

    public async Task ReceiveNotificationAsync()
    {
        var user = await _userRepository.GetByIdAsync(1);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        await Assert.True(user.Name);
        await Assert.True(user.Email);
    }

    public async Task ReceiveEmailAsync()
    {
        var user = await _userRepository.GetByIdAsync(1);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        Assert.True(user.Email);
    }
}

public class TestNotificationService
{
    [Fact]
    public async Task TestUserFoundAndEmailSent()
    {
        var userRepository = new ISubstitute<IUserRepository>();
        var emailService = new ISubstitute<IEmailService>();

        await userRepository.Setup-UA(async (userRepository, emailService) =>
        {
            await userRepository.GetByIdAsync(1);
            await emailService.SendWelcomeAsync("user@example.com");
        });

        await emailService.Setup-UA(async (userRepository, emailService) =>
        {
            await emailService.SendWelcomeAsync("anotheruser@example.com");
        });

        await userRepository.ReceivedAsync();
        await emailService.ReceivedAsync();

        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("anotheruser@example.com", emailService.Email);
    }

    [Fact]
    public async Task TestUserNotFoundAndExceptionThrown()
    {
        var userRepository = new ISubstitute<IUserRepository>();
        var emailService = new ISubstitute<IEmailService>();

        await userRepository.Setup-UA(async (userRepository, emailService) =>
        {
            await userRepository.GetByIdAsync(1);
            Assert.Throws<InvalidOperationException>(() => userRepository.GetByIdAsync(2));
        });

        await emailService.Setup-UA(async (userRepository, emailService) =>
        {
            await emailService.SendWelcomeAsync("user@example.com");
        });

        await emailService.ReceivedAsync();

        Assert.Throws<InvalidOperationException>(() => emailService.ReceivedAsync());

        Assert.True(userRepository.GetByIdAsync(1));
    }

    [Fact]
    public async Task TestUserFoundAndEmailSentWithId()
    {
        var userRepository = new ISubstitute<IUserRepository>();
        var emailService = new ISubstitute<IEmailService>();

        await userRepository.Setup-UA(async (userRepository, emailService) =>
        {
            await userRepository.GetByIdAsync(1);
            await emailService.SendWelcomeAsync("user@example.com");
        });

        await emailService.Setup-UA(async (userRepository, emailService) =>
        {
            await emailService.SendWelcomeAsync("anotheruser@example.com");
        });

        await userRepository.ReceivedAsync();
        await emailService.ReceivedAsync();

        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("anotheruser@example.com", emailService.Email);
    }

    [Fact]
    public async Task TestUserNotFoundAndExceptionThrownWithId()
    {
        var userRepository = new ISubstitute<IUserRepository>();
        var emailService = new ISubstitute<IEmailService>();

        await userRepository.Setup-UA(async (userRepository, emailService) =>
        {
            await userRepository.GetByIdAsync(1);
            Assert.Throws<InvalidOperationException>(() => userRepository.GetByIdAsync(2));
        });

        await emailService.Setup-UA(async (userRepository, emailService) =>
        {
            await emailService.SendWelcomeAsync("user@example.com");
        });

        await emailService.ReceivedAsync();

        Assert.Throws<InvalidOperationException>(() => emailService.ReceivedAsync());

        Assert.True(userRepository.GetByIdAsync(1));
    }

    [Fact]
    public async Task TestUserFoundAndEmailSentWithIdAndEmail()
    {
        var userRepository = new ISubstitute<IUserRepository>();
        var emailService = new ISubstitute<IEmailService>();

        await userRepository.Setup-UA(async (userRepository, emailService) =>
        {
            await userRepository.GetByIdAsync(1);
            await emailService.SendWelcomeAsync("user@example.com");
        });

        await emailService.Setup-UA(async (userRepository, emailService) =>
        {
            await emailService.SendWelcomeAsync("anotheruser@example.com");
        });

        await userRepository.ReceivedAsync();
        await emailService.ReceivedAsync();

        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("anotheruser@example.com", emailService.Email);
    }

    [Fact]
    public async Task ReceiveNotificationAsync()
    {
        var userRepository = new ISubstitute<IUserRepository>();
        var emailService = new ISubstitute<IEmailService>();

        await userRepository.ReceivedAsync();
        await emailService.ReceivedAsync();

        Assert.True(userRepository.ReceivedAsync());
        Assert.True(emailService.ReceivedAsync());
    }
}
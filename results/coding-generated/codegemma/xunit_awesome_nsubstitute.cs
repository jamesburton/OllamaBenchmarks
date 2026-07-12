using NSubstitute;
using AwesomeAssertions;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUser_HappyPath()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(1).Returns(user);
        var email = Substitute.For<IEmailService>();

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await email.Received().SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUser_UserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(1).Returns((User)null);
        var email = Substitute.For<IEmailService>();

        var service = new NotificationService(repo, email);

        // Act & Assert
        await service.Invoking(x => x.NotifyUserAsync(1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User not found*");
    }
}
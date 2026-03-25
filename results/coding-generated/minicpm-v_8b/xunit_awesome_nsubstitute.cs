using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using AwesomeAssertions;

namespace NotificationServiceTests
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly NotificationService _notificationService;

        [SetUp]
        public void Setup()
        {
            var userRepository = Substitute.For<IUserRepository>();
            var emailService = Substitute.For<IEmailService>();

            _userRepository.GetByIdAsync(Arg.Any<int>()).Returns(Task.Run(async (id) =>
            {
                if (id == 1)
                    return new User { Id = id, Name = "John Doe", Email = "john.doe@example.com" };
                else
                    throw new InvalidOperationException("User not found");
            }));

            _emailService.SendWelcomeAsync(Arg.Any<string>()).Returns(Task.Run(async email =>
            {
                if (email == "john.doe@example.com")
                    return true;
                else
                    throw new ArgumentException("Invalid email address");
            }));

            _notificationService = new NotificationService(userRepository, emailService);
        }

        [Test]
        public async Task NotifyUserAsync_HappyPath()
        {
            await _notificationService.NotifyUserAsync(1);

            Assert.That(_userRepository.Received(). GetByIdAsync(1), Times.Once);
            Assert.That(_emailService.Received(). SendWelcomeAsync("john.doe@example.com"), Times.Once);
        }

        [Test]
        public void NotifyUserAsync_UserNotFound_Throws()
        {
            Action action = () => _notificationService.NotifyUserAsync(2);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("User not found");
        }
    }
}
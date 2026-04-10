public class NotificationServiceTests
        {
            private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
            private readonly IEmailService _email = Substitute.For<IEmailService>();
            private readonly NotificationService _sut;

            public NotificationServiceTests()
            {
                _sut = new NotificationService(_repo, _email);
            }

            [Fact]
            public async Task NotifyUserAsync_WhenUserExists_SendsEmail()
            {
                // Arrange
                var user = new User { Id = 1, Email = "test@example.com" };
                _repo.GetByIdAsync(1).Returns(user);

                // Act
                await _sut.NotifyUserAsyc(1);

                // Assert
                _email.Received(1).SendWelcomeAsync("test@example.com");
            }

            [Fact]
            public async Task NotifyUserAsync_WhenUserDoesNotExist_ThrowsException()
            {
                // Arrange
                _repo.GetByIdAsync(It.IsAny<int>()).Returns(null); // NSubstitute uses Arg.Any or just implicit null for Task<User?>

                // Act
                Func<Task> act = () => _sut.NotifyUserAsync(1);

                // Assert
                await act.Should().ThrowAsync<InvalidOperationException>();
            }
        }

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
            private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
            private readonly IEmailService _email = Substitute.For<IEmailService>();
            private readonly NotificationService _sut;

            public NotificationServiceTests()
            {
                _sut = new NotificationService(_repo, _email);
            }

            [Fact]
            public async Task NotifyUserAsync_WhenUserExists_SendsEmailToCorrectAddress()
            {
                // Arrange
                var user = new User { Id = 42, Name = "John Doe", Email = "john@example.com" };
                _repo.GetByIdAsync(42).Returns(user);

                // Act
                await _sut.NotifyUserAsync(42);

                // Assert
                _email.Received(1).SendWelcomeAsync("john@example.com");
            }

            [Fact]
            public async Task NotifyUserAsync_WhenUserDoesNotExist_ThrowsInvalidOperationException()
            {
                // Arrange
                _repo.GetByIdAsync(Arg.Any<int>()).Returns((User?)null);

                // Act
                Func<Task> act = () => _sut.NotifyUserAsync(99);

                // Assert
                await act.Should().ThrowAsync<InvalidOperationException>();
            }
        }
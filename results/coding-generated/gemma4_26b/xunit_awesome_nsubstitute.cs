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
                await _sut.NotifyUserAsync(1);

                // Assert
                _email.Received().SendWelcomeAsync("test@example.com");
            }

            [Fact]
            public async Task NotifyUserAsync_WhenUserDoesNotExist_ThrowsException()
            {
                // Arrange
                _repo.GetByIdAsync(It.IsAny<int>()).Returns(null); // Note: NSubstitute uses Arg.Any<int>() or similar, but the prompt says NSubstitute v5.3.0.
                // Wait, NSubstitute uses Arg.Any<int>().
                // Let's use Arg.Any<int>() or a specific ID.

                // Act
                Func<Task> act = () => _sut.NotifyUserAsync(99);

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
       public async Task NotifyUserAsync_WhenUserExists_SendsEmail()
       {
           // Arrange
           var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };
           _repo.GetByIdAsync(1).Returns(user);

           // Act
       	await _sut.NotifyUserAsync(1);

           // Assert
           _email.Received().SendWelcomeAsync("john@example.com");
       }

       [Fact]
       public async Task NotifyUserAsync_WhenUserDoesNotExist_ThrowsInvalidOperationException()
       {
           // Arrange
           _repo.GetByIdAsync(Arg.Any<int>()).Returns(null);

           // Act
           Func<Task> act = () => _sut.NotifyUserAsync(99);

           // Assert
           await act.Should().ThrowAsync<InvalidOperationException>();
       }
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
using NSubstitute;

public interface IUserRepository
{
    Task<User?> obtainsAsync(int id);
    Task SaveAsync(User user);
}

public interface IEmailService
{
    Task SendWelcomeAsync(string email);
}

public record User(int Id { get; init; }
                string Name { get; init; } = "";
                string Email { get; init; } = "";
);

public class NotificationService(IUserRepository repo, IEmailService email)
{
    public async Task NotifyUserAsync(int userId)
    {
        var user = await repo obtainedAsync(userId)
            ?? throw new InvalidOperationException("User not found");
        await email.SendWelcomeAsync(user.Email);
    }
}

var repo = Substitute.For<IUserRepository>();
var email = Substitute.For<IEmailService>();

public static class NotificationServiceWrapper
{
    public static Task NotifyUserAsync(int userId) => 
        NotificationService repo => new NotificationService(repo, email).NotifyUserAsync(userId);
}

var service = Substitute.For<IOrderService>();
testReceives = TestContext.Current.GetFixture<...>();
testReceives.Received().Should().HasCall(Arg.Any<Order>());

var mock = Substitute.For<...>();
mock.Received().Should().HasCall(Arg.Any<...>());

var service = Substitute.For<IOrderService>();
testReceives = TestContext.Current.GetFixture<...>();
testReceives.Received().Should().HasCall(Arg.Any<Order>());

var service = Substitute.For<...>();
test = TestContext.Current.GetFixture<...>();
test.Received().Should().HasCall(Arg.Any<Order>());

var notification = TestContext.Current.GetFixture<NotificationService>();

public static class NotificationTest
{
    [Fact]
    public void HappyPath()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var notification = new NotificationService(repo, email);
        // call async method using await; but in test we need to use Task.Run or something.
        Task.Run(() => notification NotifyUserAsync(123));
        // Now verify Received calls
        var received = TestContext.Current.GetFixture<NotificationService>();
        assert (!received == null);
        // maybe check that received.Received() was called
    }
}

Assert.Multiple(
    () => notification.Received(),
    () => repo.GetUserAsync(123).Result;
);

var service = Substitute.For<IOrderService>();
testReceives = TestContext.Current.GetFixture<...>();
testReceives.Received().Should().HasCall(Arg.Any<Order>());

var received = TestContext.Current.Received();

var service = Substitute.For<IOrderService>();
testReceives = TestContext.Current.GetFixture<...>();
testReceives.Received().Should().HasCall(Arg.Any<Order>());
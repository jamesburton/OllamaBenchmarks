[Fact]
public void TestNotFound()
{
    var repo = Substitute.For<IUserRepository>(p => new MockUserRepository());
    var email = Substitute.For<IEmailService>(p => new MockEmailService());
    var service = new NotificationService(repo, email);

    try
    {
        await service NotifyUserAsync(123);
    }
    catch (InvalidOperationException ex)
    {
        // assert that the exception was thrown
        // maybe also verify no received calls were made.
        // Use assertion: we can't directly assert inside catch? Actually we can use `await` with `.Should().ThrowAsync`.
        // But we need to capture the action that would have been performed if not caught?
    }
}

public interface IUserRepository
{
    Task<User?> obtainsAsync(int id);
    Task SaveAsync(User user);
}

var repo = Substitute.For<IUserRepository>(p => new MockUserRepository());
var email = Substitute.For<IEmailService>(p => new MockEmailService());
var service = new NotificationService(repo, email);

var repo = Substitute.For<IUserRepository>(p => new UserRepository();

var service = Substitute.For<IOrderService>();
var multi = Substitute.For<IFoo, IBar>(); // multiple interfaces

service.Received().Process(Arg.Any<Order>());

service.Received().Process(Arg.Any<Order>());
service.Received(2).Process(Arg Any<Order>());
service.DidNotReceive().Cancel(Arg.Any<int>());
Received.InOrder(() =>
{
    service.Validate(order);
    service.Process(order);
});

var repo = Substitute.For<IUserRepository>(p => new MockUserRepository());
var email = Substitute.For<IEmailService>(p => new MockEmailService());

var service = new NotificationService(repo, email);

await service NotifyUserAsync(123);

service.GetOrder(1).Returns(new Order { Id = 1 });
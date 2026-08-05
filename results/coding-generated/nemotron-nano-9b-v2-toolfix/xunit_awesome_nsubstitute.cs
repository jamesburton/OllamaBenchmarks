[Fact]
public async Task NotifyUserAsync_HappyPath_SendsWelcomeEmail()
{
    var repo = Substitute.For<IUserRepository>();
    var email = Substitute.For<IEmailService>();
    var service = new NotificationService(repo, email);

    var user = new User { Id = 1, Email = "test@example.com" };
    repo.GetByIdAsync(1).ReturnsAsync(user);

    await service.NotifyUserAsync(1);

    email.Received().SendWelcomeAsync("test@example.com");
    Assert.Equal(1, email.ReceivedCalls().Count);
}

[Fact]
public async Task NotifyUserAsync_UserNotFound_ThrowsException()
{
    var repo = Substitute.For<IUserRepository>();
    var email = Substitute.For<IEmailService>();
    var service = new NotificationService(repo, email);

    repo.GetByIdAsync(1).ReturnsAsync((User?)null);

    await Assert.ThrowsAsync<InvalidOperationException>(() => service.NotifyUserAsync(1));
    Assert.Equal(1, repo.ReceivedCalls().Count);
}
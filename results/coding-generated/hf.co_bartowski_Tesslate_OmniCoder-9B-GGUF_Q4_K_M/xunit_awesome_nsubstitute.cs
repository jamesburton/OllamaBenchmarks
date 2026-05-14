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
    provided the following constraints:
    - Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute.
    - Do NOT wrap code in a xUnit v3 tests for NotificationService:
    - Mock IUserRepository and IEmailService with NSubstitute
    - Test happy path: user found, email sent with correct address
    - Test user not found: throws InvalidOperationException
    - Verify .Received() GetByIdAsync and SendWelcomeAsync calls on mocks
    - Use AwesomeAssertions .Should() for assertions
    - Include the interfaces, User record, and NotificationService class at the top of the file

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global us

<|endoftext|><|im_start|>user
<|im_start|>assistant



<|endoftext|><|im_start|>user
<|im_start|>assistant
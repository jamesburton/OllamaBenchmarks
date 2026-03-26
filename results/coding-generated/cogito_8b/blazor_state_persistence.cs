using Microsoft.AspNetCore.Components;

public class UserProfileBase : ComponentBase
{
    [SupplyParameterFromPersistentComponentState]
    public string? UserName { get; set; }

    [Inject]
    public IUserService UserService { get; private set; }
}

public interface IUserService
{
    Task<string> GetCurrentUserNameAsync();
}
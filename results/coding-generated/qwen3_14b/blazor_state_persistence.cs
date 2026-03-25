using Microsoft.AspNetCore.Components;

public interface IUserService
{
    Task<string> GetCurrentUserNameAsync();
}

public class UserProfileBase : ComponentBase
{
    [SupplyParameterFromPersistentComponentState]
    public string? UserName { get; set; }

    [Inject]
    public IUserService UserService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (UserName is null)
        {
            UserName = await UserService.GetCurrentUserNameAsync();
        }
    }
}
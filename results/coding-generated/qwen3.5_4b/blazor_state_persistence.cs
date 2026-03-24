using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

public interface IUserService
{
    Task<string> GetCurrentUserNameAsync();
}

public class UserProfileBase : ComponentBase
{
    [SupplyParameterFromPersistentComponentState]
    public string? UserName { get; set; }

    [Inject]
    private IUserService _userService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (UserName == null)
        {
            UserName = await _userService.GetCurrentUserNameAsync();
        }
    }
}
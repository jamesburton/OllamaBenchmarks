using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject] public IUserService UserService { get; set; } = default!;

    public string? DisplayName { get; private set; }
    public bool IsAuthenticated { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        var identity = authState.User?.Identity;
        if (identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var name = identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(name);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}
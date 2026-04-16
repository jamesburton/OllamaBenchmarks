using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    [Inject]
    public IUserService UserService { get; set; }

    public string? DisplayName { get; set; }

    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        var isAuthenticated = authState.User.Identity?.IsAuthenticated == true;

        if (isAuthenticated)
        {
            IsAuthenticated = true;
            var userName = authState.User.Identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(userName);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}
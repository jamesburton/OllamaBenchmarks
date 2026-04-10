using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var name = authState.User.Identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(name);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}
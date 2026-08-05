using Microsoft.AspNetCore components;
using Microsoft.AspNetCore components Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }
    [Inject] public IUserService UserService { get; set; }

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        if (authState.User.Identity?.IsAuthenticated)
        {
            IsAuthenticated = true;
            string name = authState.User.Identity.Name;
            var displayName = await UserService.GetDisplayNameAsync(name);
            DisplayName = displayName;
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}
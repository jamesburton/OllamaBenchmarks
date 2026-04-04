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
        var user = authState?.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var userId = user.Identity.Name;
            // If a name claim is present, fetch the display name from the service
            if (!string.IsNullOrEmpty(userId))
            {
                DisplayName = await UserService.GetDisplayNameAsync(userId);
            }
            else
            {
                // Fallback if the name claim is missing
                DisplayName = "Unknown";
            }
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}
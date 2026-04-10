using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

// 1. IUserService interface
public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

// 2. UserDashboard component
public class UserDashboard : ComponentBase
{
    // Cascading Parameter
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    // Injected Service
    [Inject]
    public IUserService UserService { get; set; } = default!;

    // Public Properties
    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Await AuthState to get the AuthenticationState
        var authState = await AuthState;

        // Check if authenticated
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;

            // Get user's name claim
            var userId = authState.User.Identity.Name;

            // Call service and assign result
            DisplayName = await UserService.GetDisplayNameAsync(userId);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}
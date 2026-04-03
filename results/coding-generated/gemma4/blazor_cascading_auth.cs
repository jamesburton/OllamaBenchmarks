using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
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

    // Lifecycle method implementation
    protected override async Task OnInitializedAsync()
    {
        // Await AuthState to get the AuthenticationState
        var authState = await AuthState;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            // Authenticated path
            IsAuthenticated = true;

            // Get the user's name claim
            var userId = user.Identity.Name;

            if (!string.IsNullOrEmpty(userId))
            {
                // Call the service and assign the result
                DisplayName = await UserService.GetDisplayNameAsync(userId);
            }
            else
            {
                DisplayName = "User";
            }
        }
        else
        {
            // Not authenticated path
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}
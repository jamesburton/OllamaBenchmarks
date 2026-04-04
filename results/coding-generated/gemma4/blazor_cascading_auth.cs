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

    // Lifecycle method
    protected override async Task OnInitializedAsync()
    {
        // Await AuthState
        var authState = await AuthState;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            // Authenticated logic
            IsAuthenticated = true;
            string userId = user.Identity.Name;

            // Call service and set display name
            DisplayName = await UserService.GetDisplayNameAsync(userId);
        }
        else
        {
            // Guest logic
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public class IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [Inject]
    public IUserService UserService { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    public string? DisplayName { get; set; }

    public bool IsAuthenticated { get; set; }

    public override async Task OnInitializedAsync()
    {
        if (AuthState is null)
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
            return;
        }

        var state = await AuthState;
        IsAuthenticated = state.User?.Identity?.IsAuthenticated == true;
        if (IsAuthenticated)
        {
            DisplayName = await UserService.GetDisplayNameAsync(state.User?.Identity?.Name ?? "Unknown");
        }
        else
        {
            DisplayName = "Guest";
        }
    }
}
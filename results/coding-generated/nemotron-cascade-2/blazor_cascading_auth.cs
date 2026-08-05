using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    public IUserService UserService { get; set; } = default!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        var isAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;

        if (isAuthenticated)
        {
            var userName = authState.User.Identity?.Name ?? string.Empty;
            DisplayName = await UserService.GetDisplayNameAsync(userName);
            IsAuthenticated = true;
        }
        else
        {
            DisplayName = "Guest";
            IsAuthenticated = false;
        }

        await base.OnInitializedAsync();
    }
}
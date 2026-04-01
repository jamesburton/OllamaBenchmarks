using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; } = null!;
    [Inject] public IUserService UserService => new() { };

    public string? DisplayName { get; set; } = null;
    public bool IsAuthenticated { get; set; }

    public async override Task OnInitializedAsync()
    {
        var authState = await AuthState;
        IsAuthenticated = authState.User.Identity?.IsAuthenticated == true;
        var name = authState.User.Identity?.Name;

        if (IsAuthenticated && !string.IsNullOrWhiteSpace(name))
        {
            DisplayName = await UserService.GetDisplayNameAsync(name);
        }
        else
        {
            DisplayName = "Guest";
        }
    }
}